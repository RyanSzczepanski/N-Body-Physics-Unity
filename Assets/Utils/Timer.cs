using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private static float startTime;
    private static float endTime;

    public void Start()
    {
        startTime = Time.realtimeSinceStartup;
    }

    public void Stop()
    {
        endTime = Time.realtimeSinceStartup;
        Debug.Log($"{(endTime - startTime) * 1000} ms");
    }
}
