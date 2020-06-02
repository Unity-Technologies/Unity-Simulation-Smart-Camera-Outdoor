using System.Collections;
using System.Collections.Generic;
using Unity.Simulation;
using UnityEngine;

public class AppParamsReader : MonoBehaviour
{
    public struct SimulationConfig
    {
        public int maxNumberOfFrames;
        public int numberOfCars;
        public bool captureDrivingLogs;
        public bool captureDepthData;
        public float lightIntensity;
        public string daytime;
        public string cameraViewToCapture;
    }
    
    [RuntimeInitializeOnLoadMethod]
    public static void LoadAppParams()
    {
        if (Configuration.Instance.IsSimulationRunningInCloud())
        {
            var config = Configuration.Instance.GetAppParams<SimulationConfig>();
            SimulationOptions.MaxNumberofCars = config.numberOfCars;
            SimulationOptions.MaxNumberOfFramesToCapture = config.maxNumberOfFrames;
            SimulationOptions.CaptureDrivingLogs = config.captureDrivingLogs;
            SimulationOptions.LightIntensity = config.lightIntensity;
            SimulationOptions.Daytime = config.daytime;
            SimulationOptions.CaptureDepthData = config.captureDepthData;
            SimulationOptions.CameraViewToCapture = config.cameraViewToCapture;
        }
    }
}


public class SimulationOptions
{
    public static int MaxNumberofCars = 10;
    public static int MaxNumberOfFramesToCapture = 5000;
    public static bool CaptureDrivingLogs = false;
    public static bool CaptureDepth;
    public static float LightIntensity = 1.0f;
    public static string Daytime = "morning";
    public static bool CaptureDepthData = true;
    public static string CameraViewToCapture = "Intersection";
}
