using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Client;

public class ClientMenu : MonoBehaviour
{
    [MenuItem("Simulation/Build Project")]
    public static void BuildProject()
    {
        var scenes = new string[]
        {
            "Assets/UnityScenarios/SceneAssets/Scenes/CityScene.unity"
        };
        Project.BuildProject("./build", "build", scenes);
    }

    [MenuItem("Simulation/Execute Run")]
    public static void Setup()
    {
        var run = Run.Create("Smart_Camera", "Run");
        var sysParam = API.GetSysParams()[0];
        run.SetSysParam(sysParam);
        var buildPath = System.IO.Path.Combine(Application.dataPath, "..", "build.zip");
        if (File.Exists(buildPath)) run.SetBuildLocation(buildPath); 
        else Debug.LogError("Build file does not exist at: " + buildPath); 
        var param_path = "/StreamingAssets/default_app_param.json";
        string json = "";
        if (File.Exists(Application.dataPath + param_path)) json = File.ReadAllText(Application.dataPath + param_path);
        else {
            Debug.LogError("App param file does not exist at: " + Application.dataPath + param_path); 
            return;
        }
        var content = JsonUtility.FromJson<SimulationOptions>(json);
        run.SetAppParam("default", content, 1);
        run.Execute();
    }
}
