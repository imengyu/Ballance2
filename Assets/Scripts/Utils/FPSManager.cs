﻿using UnityEngine;
using UnityEngine.UI;

public class FPSManager : MonoBehaviour
{
    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames = 0;
    private int tick = 0;
    public float fps;
    public Text FpsText;

    void Awake()
    {
        //		Application.runInBackground = true;
        //		Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //		Application.targetFrameRate = 30;
    }
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
        if (tick < 60) tick++;
        else
        {
            tick = 0;
            if (FpsText != null) FpsText.text = fps.ToString("0.0");
        }
    }

}
