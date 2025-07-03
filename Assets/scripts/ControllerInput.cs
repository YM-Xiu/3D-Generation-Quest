using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : MonoBehaviour
{
    private InputActions input;

    [Tooltip("Distance in front of the camera to spawn the prefab.")]
    public float spawnDistance = 0.3f;

    // Path must be relative to Resources folder and *case-sensitive*
    [Tooltip("Path to the spawn prefab under Resources (e.g., 'PREFABS/BB_C')")]
    public string bbCPrefabPath = "PREFABS/GenerationCube";
    private GameObject lastCubeInstance;

    void Awake()
    {
        input = new InputActions();
    }

    void OnEnable()
    {
        input.Controller.TriggerRecord.performed += OnRecordPressed;
        input.Controller.TriggerAudio.performed += OnAudioPressed;
        input.Enable();
    }

    void OnDisable()
    {
        input.Controller.TriggerRecord.performed -= OnRecordPressed;
        input.Controller.TriggerAudio.performed -= OnAudioPressed;
        input.Disable();
    }

    void OnRecordPressed(InputAction.CallbackContext ctx)
    {
        SpawnBBC();
    }

    void OnAudioPressed(InputAction.CallbackContext ctx)
    {
        if (lastCubeInstance != null)
        {
            var cubeRecord = lastCubeInstance.GetComponent<CubeRecord>();
            if (cubeRecord != null)
                cubeRecord.ToggleRecording();
            else
                Debug.LogWarning("CubeRecord component not found on cube.");
        }
        else
        {
            Debug.LogWarning("No cube has been instantiated yet.");
        }
    }

    void SpawnBBC()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[ControllerInput] Main Camera not found.");
            return;
        }

        Vector3 spawnPosition = cam.transform.position + cam.transform.forward * spawnDistance;
        Quaternion spawnRotation = Quaternion.LookRotation(cam.transform.forward);

        GameObject bbCPrefab = Resources.Load<GameObject>(bbCPrefabPath);
        if (bbCPrefab == null)
        {
            Debug.LogError($"[ControllerInput] Could not load gen cube prefab at Resources/{bbCPrefabPath}");
            return;
        }

        GameObject instance = Instantiate(bbCPrefab, spawnPosition, spawnRotation);
        instance.name = "BB_C_Instance";

        lastCubeInstance = instance;

        Debug.Log("[ControllerInput] generation cube prefab spawned.");
    }
}
