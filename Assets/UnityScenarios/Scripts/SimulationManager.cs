using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{

    [Header("Camera Selection")] 
    public bool m_EnableCarDashboardCamera;
    
    [Header("Additional Traffic Car Spawning")]
    public GameObject       m_CarPrefab;
    public int              m_NumberOfCars;
    public List<SpawnPoint> m_SpawnPoints;
    public Material[]       m_CarMaterials;


    public GameObject CurrentCameraView;

    public ScriptableObject m_LablelingConfiguration;
    
    [Header("Car Perception Camera")] 
    public GameObject m_CarCamera;
    
    
    [Header("Intersection Perception Camera")]
    public GameObject m_IntersectionCamera;
    
    
    private const int MaxCarsAllowed = 10;


    [Serializable]
    public struct SpawnPoint
    {
        public Transform node;
        public int startingDest;
        public Transform path;
    }

    private void Start()
    {
        StartCoroutine(SpawnCars());
        CurrentCameraView = m_EnableCarDashboardCamera ? m_CarCamera : m_IntersectionCamera;
        m_CarCamera.SetActive(m_EnableCarDashboardCamera);
        m_IntersectionCamera.SetActive(!m_EnableCarDashboardCamera);
    }

    /// <summary>
    /// Spawn cars at Spawn points at an interval of 25s of simulation time.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SpawnCars()
    {
        var numberOfCars = m_NumberOfCars;

        if (Configuration.Instance.IsSimulationRunningInCloud())
        {
            numberOfCars = Math.Min(SimulationOptions.MaxNumberofCars, MaxCarsAllowed);
        }

        for (int i = 0; i < numberOfCars; i++)
        {
            var spawnPoint = i % m_SpawnPoints.Count;
            var car = GameObject.Instantiate(m_CarPrefab);
            var renderer = (Renderer) car.transform.GetComponentInChildren(typeof(Renderer));
            renderer.material = m_CarMaterials[Random.Range(0, m_CarMaterials.Length - 1)];
            
            car.transform.position = m_SpawnPoints[spawnPoint].node.position;
            var pathFollow = (PathFollow)car.GetComponentInChildren(typeof(PathFollow));
            pathFollow.startingPoint = m_SpawnPoints[spawnPoint].startingDest;
            pathFollow.path = m_SpawnPoints[spawnPoint].path;

            if (spawnPoint == 0)
                yield return new WaitForSeconds(50.0f);
        }

    }


    /// <summary>
    /// Add Label to GameObject spawned at runtime.
    /// </summary>
    /// <param name="go"></param>
    public void AddLabelingToGameObject(GameObject go)
    {
        Debug.Log(go.name);
        Debug.Assert(!String.IsNullOrEmpty(go.tag), "The GameObject is not tagged");
        var pcam = CurrentCameraView.GetComponent<PerceptionCamera>();
        var labelingComponent = go.AddComponent<Labeling>();
        labelingComponent.labels = new List<string>() { tag };
    }
    
}


#if UNITY_EDITOR
[CustomEditor(typeof(SimulationManager))]
public class SimulationManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SimulationManager manager = (SimulationManager) target;

        if (manager.m_EnableCarDashboardCamera)
        {
            manager.GetComponent<ApplyLabeling>().m_Camera = manager.m_CarCamera.GetComponent<Camera>();
            manager.m_CarCamera.SetActive(true);
            manager.m_IntersectionCamera.SetActive(false);
        }
        else
        {
            manager.GetComponent<ApplyLabeling>().m_Camera = manager.m_IntersectionCamera.GetComponent<Camera>();
            manager.m_CarCamera.SetActive(false);
            manager.m_IntersectionCamera.SetActive(true);
        }
    }
}
#endif
