using UnityEngine;
using GLTFast;
using System.Threading.Tasks;
using TMPro;
public class GLBLoader : MonoBehaviour
{
    public async void LoadGLB(string path)
    {
        GltfImport gltf = new GltfImport();
        bool success = await gltf.Load(path);

        if (!success)
        {
            Debug.LogError("Failed to load GLB");
            return;
        }

        // Create a container GameObject to hold the GLB
        GameObject glbObject = new GameObject("GLB_Child");
        GetComponentInChildren<TextMeshPro>().enabled = false;
        // Set it as a child of this GameObject (the cube)
        glbObject.transform.parent = transform;
        glbObject.transform.localPosition = Vector3.zero;
        glbObject.transform.localRotation = Quaternion.identity;
        glbObject.transform.localScale = Vector3.one;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        // Instantiate the GLB under the container
        gltf.InstantiateMainScene(glbObject.transform);
        Renderer[] renderers = glbObject.GetComponentsInChildren<Renderer>();
foreach (Renderer renderer in renderers)
{
    // For dynamic GI
    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;

    // Optional: if you have reflection probes
    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
}



    }
}
