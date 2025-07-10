using UnityEngine;

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

    public static void StopAndLogTimer()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("⚠️ Timer was not running.");
            return;
        }

        float elapsedTime = Time.realtimeSinceStartup - StartTime;
        Debug.Log($"✅ Elapsed time: {elapsedTime} seconds");
        IsRunning = false;
    }
}
