using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PerfMeasure : MonoBehaviour
{
    int startFrameCount = 0;
    float Fps;
    float startTime = 0;
    private void Start()
    {
        startTime = Time.realtimeSinceStartup;
    }
    private float GetFps()
    {
        return (Time.renderedFrameCount - startFrameCount) / Time.unscaledDeltaTime;
    }
    private void SampleFps()
    {
        Fps = GetFps();
        startFrameCount = Time.renderedFrameCount;
    }
    void Update()
    {
        SampleFps();
        if (Time.renderedFrameCount == SimulationOptions.MaxNumberOfFramesToCapture)
        {
            Debug.Log($"Fps: {Fps} Wall Time lapsed: {Time.realtimeSinceStartup - startTime}");
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}