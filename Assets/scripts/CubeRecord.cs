using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CubeRecord : MonoBehaviour
{
    private InputActions input;
    private AudioSource audioSource;
    public AudioRecorder recorder;
    [ Header("Audio Hint Settings")]
    [Tooltip("Relative path in Resources to the audio clip (e.g., 'Audio/MyAudioClip')")]
    public string startClip;
    [Tooltip("Relative path in Resources to the audio clip (e.g., 'Audio/MyAudioClip')")]

    public string stopClip;
    public bool playHints = true;
    private TextMeshPro tmp;
    void Awake()
    {
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(this.transform);
        textObj.transform.localPosition = new Vector3(0, 1, 0); // position it

        tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "Press Y to prompt me";
        tmp.fontSize = 36.0f;
        textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        audioSource = gameObject.AddComponent<AudioSource>();
        Debug.LogError("awake");
        // recorder.RecordAndSend();

    }

    void OnEnable()
    {
        Debug.LogError("enable");
    }

    void OnDisable()
    {
        Debug.LogError("disable");

    }

    public void ToggleRecording()
    {
        if (recorder != null)
        {
            if (playHints)
            {
                if (recorder.getStatus())
                {
                    PlayAudioHint(stopClip);
                    tmp.text = "Loading object...";
                }
                else
                {
                    PlayAudioHint(startClip);
                    tmp.text = "Recording... (Press Y to stop)";
                }
            }
            recorder.ToggleRecording();
        }
        else
        {
            Debug.LogWarning("[CubeRecord] Recorder not assigned.");
        }
    }
    
    public void PlayAudioHint(string audioClipPath)
        {
            if (string.IsNullOrEmpty(audioClipPath))
            {
                Debug.LogWarning("[AudioHint] audioClipPath is not set.");
                return;
            }

            AudioClip clip = Resources.Load<AudioClip>(audioClipPath);
            if (clip == null)
            {
                Debug.LogError($"[AudioHint] Could not find audio clip at Resources/{audioClipPath}");
                return;
            }

            audioSource.PlayOneShot(clip);
        }
}
