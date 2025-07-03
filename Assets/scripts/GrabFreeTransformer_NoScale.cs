using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction
{
    /// <summary>
    /// A modified <see cref="ITransformer"/> that only allows translation and rotation using any number of grab points.
    /// Scaling is explicitly disabled. Use this when you want to allow movement and rotation but lock object scale.
    /// </summary>
    public class GrabFreeTransformer_NoScale : MonoBehaviour, ITransformer
    {
        [SerializeField]
        private TransformerUtils.PositionConstraints _positionConstraints =
            new TransformerUtils.PositionConstraints();

        [SerializeField]
        private TransformerUtils.RotationConstraints _rotationConstraints =
            new TransformerUtils.RotationConstraints();

        [SerializeField]
        private TransformerUtils.ScaleConstraints _scaleConstraints =
            new TransformerUtils.ScaleConstraints(); // unused

        [SerializeField]
        private bool _resetScaleResponsivenessOnConstraintOvershoot = false; // unused

        private IGrabbable _grabbable;
        private Pose _grabDeltaInLocalSpace;
        private TransformerUtils.PositionConstraints _relativePositionConstraints;

        private Quaternion _lastRotation = Quaternion.identity;
        private GrabPointDelta[] _deltas;

        internal struct GrabPointDelta
        {
            private const float _epsilon = 0.000001f;
            public Vector3 PrevCentroidOffset { get; private set; }
            public Vector3 CentroidOffset { get; private set; }
            public Quaternion PrevRotation { get; private set; }
            public Quaternion Rotation { get; private set; }

            public GrabPointDelta(Vector3 centroidOffset, Quaternion rotation)
            {
                PrevCentroidOffset = CentroidOffset = centroidOffset;
                PrevRotation = Rotation = rotation;
            }

            public void UpdateData(Vector3 centroidOffset, Quaternion rotation)
            {
                PrevCentroidOffset = CentroidOffset;
                CentroidOffset = centroidOffset;
                PrevRotation = Rotation;

                if (Quaternion.Dot(rotation, Rotation) < 0)
                {
                    rotation = new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
                }

                Rotation = rotation;
            }

            public bool IsValidAxis()
            {
                return CentroidOffset.sqrMagnitude > _epsilon;
            }
        }

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
            _relativePositionConstraints = TransformerUtils.GenerateParentConstraints(
                _positionConstraints, _grabbable.Transform.localPosition);
        }

        public void BeginTransform()
        {
            int count = _grabbable.GrabPoints.Count;
            _deltas = ArrayPool<GrabPointDelta>.Shared.Rent(count);
            InitializeDeltas(count, _grabbable.GrabPoints, ref _deltas);

            Transform targetTransform = _grabbable.Transform;
            Vector3 centroid = GetCentroid(_grabbable.GrabPoints);

            _grabDeltaInLocalSpace = new Pose(
                targetTransform.InverseTransformVector(centroid - targetTransform.position),
                targetTransform.rotation);

            _lastRotation = Quaternion.identity;
        }

        public void UpdateTransform()
        {
            int count = _grabbable.GrabPoints.Count;
            Transform targetTransform = _grabbable.Transform;

            Vector3 localPosition = UpdateTransformerPointData(_grabbable.GrabPoints, ref _deltas);

            // Skip scaling entirely

            _lastRotation = UpdateRotation(count, _deltas) * _lastRotation;
            Quaternion rotation = _lastRotation * _grabDeltaInLocalSpace.rotation;
            targetTransform.rotation = TransformerUtils.GetConstrainedTransformRotation(
                rotation, _rotationConstraints, targetTransform.parent);

            Vector3 position = localPosition - targetTransform.TransformVector(_grabDeltaInLocalSpace.position);
            targetTransform.position = TransformerUtils.GetConstrainedTransformPosition(
                position, _relativePositionConstraints, targetTransform.parent);
        }

        public void EndTransform()
        {
            ArrayPool<GrabPointDelta>.Shared.Return(_deltas);
            _deltas = null;
        }

        internal static void InitializeDeltas(int count, List<Pose> poses, ref GrabPointDelta[] deltas)
        {
            Vector3 centroid = GetCentroid(poses);
            for (int i = 0; i < count; i++)
            {
                deltas[i] = new GrabPointDelta(centroid - poses[i].position, poses[i].rotation);
            }
        }

        internal static Vector3 UpdateTransformerPointData(List<Pose> poses, ref GrabPointDelta[] deltas)
        {
            Vector3 centroid = GetCentroid(poses);
            for (int i = 0; i < poses.Count; i++)
            {
                deltas[i].UpdateData(centroid - poses[i].position, poses[i].rotation);
            }
            return centroid;
        }

        internal static Vector3 GetCentroid(List<Pose> poses)
        {
            Vector3 sum = Vector3.zero;
            foreach (var pose in poses)
            {
                sum += pose.position;
            }
            return sum / poses.Count;
        }

        internal static Quaternion UpdateRotation(int count, GrabPointDelta[] deltas)
        {
            Quaternion combined = Quaternion.identity;
            float fraction = 1f / count;

            for (int i = 0; i < count; i++)
            {
                var delta = deltas[i];
                Quaternion rotDelta = delta.Rotation * Quaternion.Inverse(delta.PrevRotation);

                if (delta.IsValidAxis())
                {
                    Vector3 axis = delta.CentroidOffset.normalized;
                    Quaternion dirDelta = Quaternion.FromToRotation(delta.PrevCentroidOffset.normalized, axis);
                    combined = Quaternion.Slerp(Quaternion.identity, dirDelta, fraction) * combined;

                    rotDelta.ToAngleAxis(out float angle, out Vector3 twistAxis);
                    float twistAmount = Vector3.Dot(twistAxis, axis);
                    rotDelta = Quaternion.AngleAxis(angle * twistAmount, axis);
                }

                combined = Quaternion.Slerp(Quaternion.identity, rotDelta, fraction) * combined;
            }

            return combined;
        }

        #region Inject

        public void InjectOptionalPositionConstraints(TransformerUtils.PositionConstraints constraints)
        {
            _positionConstraints = constraints;
        }

        public void InjectOptionalRotationConstraints(TransformerUtils.RotationConstraints constraints)
        {
            _rotationConstraints = constraints;
        }

        public void InjectOptionalScaleConstraints(TransformerUtils.ScaleConstraints constraints)
        {
            _scaleConstraints = constraints;
        }

        #endregion
    }
}
