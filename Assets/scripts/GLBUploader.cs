using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class GLBUploader : MonoBehaviour
{
    public string ServerUrl = "http://192.168.1.189:5017/generate-3d";

    public IEnumerator SendAudioToServer(string audioPath)
    {
        byte[] audioBytes = File.ReadAllBytes(audioPath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recorded.wav", "audio/wav");

        using UnityWebRequest www = UnityWebRequest.Post(ServerUrl, form);
        yield return www.SendWebRequest();

        if (audioBytes.Length > 0 || audioBytes != null)
        {
            Debug.Log("Audio file size: " + audioBytes.Length + " bytes");
        }
        else
        {
            Debug.LogError("Audio file is empty or null.");
        }

        Debug.Log("Sending request to: " + ServerUrl);


        if (www.result == UnityWebRequest.Result.Success)
        {
            string glbPath = Path.Combine(Application.persistentDataPath, "output.glb");
            File.WriteAllBytes(glbPath, www.downloadHandler.data);
            Debug.Log("3D model downloaded to: " + glbPath);

            GetComponent<GLBLoader>().LoadGLB(glbPath);
        }
        else
        {
            Debug.LogError("Upload failed: " + www.error);
            Debug.LogError("Status Code: " + www.responseCode);
            Debug.LogError("Error: " + www.error);
            Debug.LogError("Response: " + www.downloadHandler.text);

        }
    }
}

