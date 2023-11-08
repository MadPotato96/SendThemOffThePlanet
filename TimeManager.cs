using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeManager
{
    public static void SetGlobalTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public static void SetPlayerTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
