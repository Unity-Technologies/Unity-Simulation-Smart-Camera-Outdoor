using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEditor;
using Unity.Simulation.Client;

public class ClientMenu : MonoBehaviour
{

    private static Thread _thread;
    private static string _dataDownloadLocation;
    private static bool runStarted;
    private static Run run;
    private static int _runMonitorInterval = 10;
    private static float _timeElapsed = 0.0f;
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
        run = Run.Create("Smart_Camera", "Run");
        var sysParam = API.GetSysParams()[0];
        run.SetSysParam(sysParam);
        var buildPath = System.IO.Path.Combine(Application.dataPath, "..", "build.zip");
        if (File.Exists(buildPath)) run.SetBuildLocation(buildPath); 
        else Debug.LogError("Build file does not exist at: " + buildPath); 
        var param_path = "/StreamingAssets/default_app_param.json";
        string json = "";
        if (File.Exists(Application.dataPath + param_path))
        {
            json = File.ReadAllText(Application.dataPath + param_path);   
        }
        else 
        {
            Debug.LogError("App param file does not exist at: " + Application.dataPath + param_path); 
            return;
        }
        
        run.SetAppParam("simulation-app-param", json, 1);

        _dataDownloadLocation = Application.persistentDataPath + "/SimulationRuns/" + run.executionId;

        if (!Directory.Exists(_dataDownloadLocation))
            Directory.CreateDirectory(_dataDownloadLocation);

        EditorApplication.update += MonitorRunExecution;
      
        _thread = new Thread(new ThreadStart(() =>
        {
            Debug.Log("Uploading the build and scheduling the simulation run");
            if (!runStarted)
            {
                run.Execute();
                runStarted = true;
            }
        }));
        _thread.Start();
    }

    private static void MonitorRunExecution()
    {
        _timeElapsed += Time.deltaTime;
        
        if (_timeElapsed < _runMonitorInterval)
            return;

        _timeElapsed = 0;
        
        if (runStarted)
        {
            if (run.completed)
            {
                if (!Directory.Exists(_dataDownloadLocation))
                    Directory.CreateDirectory(_dataDownloadLocation);
                
                var download = EditorUtility.DisplayDialog("Simulation Run",
                    "The simulation run for " + run.executionId + " is complete", "Download Manifest");
                if (download)
                {
                    var manifest = API.GetManifest(run.executionId);

                    var wc = new WebClient();

                    foreach (var entry in manifest)
                    {
                        var e = entry.Value;

                        wc.DownloadFile(e.downloadUri, _dataDownloadLocation + "/" + e.fileName);
                    }
                }

                runStarted = false;
                EditorApplication.update -= MonitorRunExecution;
            }
        }
        else
        {
            Debug.Log("Uploading Build to USim");
        }
    }
}
