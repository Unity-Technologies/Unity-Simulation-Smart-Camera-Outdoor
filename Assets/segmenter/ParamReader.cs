//using Unity.AI.Simulation;
//using UnityEngine;
//
//public class ParamReader : MonoBehaviour
//{
//
//    private AppParam appParams;
//    private static float quitAfterSeconds = 5;
//    private static string activeLocation = "ParkCircleObjects";
//    private static float simElapsedSeconds;
//
//    void Start()
//    {
//        simElapsedSeconds = 0;
//
//        // NOTE: AppParams can be loaded anytime except during `RuntimeInitializeLoadType.BeforeSceneLoad`
//        // If the simulation is running locally load app_param_0.json
//        if (!Configuration.Instance.IsSimulationRunningInCloud())
//        {
//            Configuration.Instance.SimulationConfig.app_param_uri = "file://" + Application.dataPath + "/StreamingAssets/default_app_param.json";
//        }
//
//        Debug.Log(Configuration.Instance.SimulationConfig.app_param_uri);
//      
//
//        appParams = Configuration.Instance.GetAppParams<AppParam>();
//
//        Debug.Log(appParams.toString());
//
//        // Check if AppParam file was passed during command line execution
//        if (appParams != null)
//        {
//            // Set light intensity
//            Light directionalLight = GameObject.FindGameObjectWithTag("sun").GetComponent<Light>();
//            directionalLight.intensity = appParams.lightIntensity;
//            
//            // Set the Simulation exit time.
//            quitAfterSeconds = appParams.quitAfterSeconds;
//
//            // Set the active locations
//            activeLocation = appParams.activeLocation;
//        }
//
//
//        // Disable all child objects not at the active location
//        GameObject[] locations = GameObject.FindGameObjectsWithTag("location");
//        for (int locationNum = 0; locationNum < locations.Length; locationNum++)
//        {
//            Transform location = locations[locationNum].transform;
//            if (activeLocation != location.name)
//            {
//
//                for (int childNum = 0; childNum < location.childCount; childNum ++)
//                {
//                    location.GetChild(childNum).gameObject.SetActive(false);
//                }
//            }
//        }
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//        simElapsedSeconds += Time.deltaTime;
//        if (simElapsedSeconds > quitAfterSeconds)
//        {
//            Application.Quit();
//        }
//    }
//}