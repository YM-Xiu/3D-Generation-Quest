using UnityEngine;
using System.IO;
using System.Collections;
public class AudioRecorder : MonoBehaviour
{
    private AudioClip recordedClip;
    private string filePath;

    private bool isRecording = false;

    public void ToggleRecording()
    {
        if (!isRecording)
        {
            Debug.Log("[AudioRecorder] Start recording");
            StartCoroutine(StartRecordingRoutine());
        }
        else
        {
            Debug.Log("[AudioRecorder] Stop recording");
            StopRecording();
        }
    }

    // public void StartRecording()
    // {
    //     recordedClip = Microphone.Start(null, false, 5, 44100);
    // }

    private IEnumerator StartRecordingRoutine()
    {
        isRecording = true;
        recordedClip = Microphone.Start(null, false, 10, 44100); // Max 10s duration
        yield return null; // optional wait
    }

    // public void StopRecording()
    // {
    //     Microphone.End(null);
    //     filePath = Path.Combine(Application.persistentDataPath, "recorded.wav");
    //     SaveWav(filePath, recordedClip);
    //     StartCoroutine(GetComponent<GLBUploader>().SendAudioToServer(filePath));
    // }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        Microphone.End(null);
        filePath = Path.Combine(Application.persistentDataPath, "recorded.wav");
        SaveWav(filePath, recordedClip);
        StartCoroutine(GetComponent<GLBUploader>().SendAudioToServer(filePath));
    }

    // Utility to save clip to WAV
    public static void SaveWav(string filepath, AudioClip clip)
    {
        var wavData = WavUtility.FromAudioClip(clip); // use any WAV utility like https://github.com/deadlyfingers/UnityWav
        File.WriteAllBytes(filepath, wavData);
    }
    public bool getStatus()
    {
        return isRecording;
    }
    // public void RecordAndSend()
    // {
    //     Debug.LogError("recordandsend");
    //     StartCoroutine(RecordThenSend());
    // }

    // private IEnumerator RecordThenSend()
    // {
    //     Debug.LogError("recordthensend");
    //     StartRecording();
    //     yield return new WaitForSeconds(5f);
    //     StopRecording();
    // }

}

