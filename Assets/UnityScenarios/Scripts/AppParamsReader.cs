using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Simulation;
using UnityEngine;

public class AppParamsReader : MonoBehaviour
{
    [Serializable]
    public class SimulationConfig
    {
        public int maxNumberOfFrames;
        public int numberOfCars;
        public float lightIntensity;
        public string daytime;
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void LoadAppParams()
    {
       if (Configuration.Instance.IsSimulationRunningInCloud())
       {
            var config = Configuration.Instance.GetAppParams<SimulationConfig>();
            SimulationOptions.MaxNumberofCars = config.numberOfCars;
            SimulationOptions.MaxNumberOfFramesToCapture = config.maxNumberOfFrames;
            SimulationOptions.LightIntensity = config.lightIntensity;
            SimulationOptions.Daytime = config.daytime;
       }
    }
}


public class SimulationOptions
{
    public static int MaxNumberofCars = 10;
    public static int MaxNumberOfFramesToCapture = 500;
    public static bool CaptureDepth;
    public static float LightIntensity = 1.0f;
    public static string Daytime = "morning";
}
