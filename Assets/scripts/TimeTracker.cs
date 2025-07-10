using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public static class TimerTracker
{
    public static float StartTime;
    public static bool IsRunning = false;

    public static void StartTimer()
    {
        StartTime = Time.realtimeSinceStartup;
        IsRunning = true;
        Debug.Log("⏱️ Timer started.");
    }

    public static void StopAndLogTimer(MonoBehaviour context)
    {
        if (!IsRunning)
        {
            Debug.LogWarning("⚠️ Timer was not running.");
            return;
        }

        float elapsedTime = Time.realtimeSinceStartup - StartTime;
        Debug.Log($"✅ Elapsed time: {elapsedTime} seconds");

        IsRunning = false;

        // Start coroutine to send data
        context.StartCoroutine(PostElapsedTime(elapsedTime));
    }

    private static IEnumerator PostElapsedTime(float elapsedTime)
    {
        string url = "http://192.168.1.189:5017/update-latest-row"; // Replace with your Flask server URL

        string jsonPayload = JsonUtility.ToJson(new ElapsedTimeData
        {
            column = "elapsed_time_seconds",
            value = elapsedTime
        });

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Failed to send elapsed time: " + request.error);
        }
        else
        {
            Debug.Log("📤 Elapsed time sent successfully: " + request.downloadHandler.text);
        }
    }

    [System.Serializable]
    private class ElapsedTimeData
    {
        public string column;
        public float value;
    }
}
