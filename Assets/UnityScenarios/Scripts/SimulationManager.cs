using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using Object = System.Object;
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
    
    public string drivingInfoMetricId = "B3671CF5-088E-42C5-9C8F-54C65A75CD77";
    
    [Header("Car Perception Camera")] 
    public GameObject m_CarCamera;
    
    
    [Header("Intersection Perception Camera")]
    public GameObject m_IntersectionCamera;
    
    
    private const int MaxCarsAllowed = 10;
    
    private MetricDefinition _drivingLogMetricDefinition;
    private List<PathFollow> _carPathFollows = new List<PathFollow>(); 


    [Serializable]
    public struct SpawnPoint
    {
        public Transform node;
        public int startingDest;
        public Transform path;
    }

    struct DrivingInfo
    {
        public Vector3 position;
        public float steer;
        public double speed;
    }

    private void Start()
    {
        StartCoroutine(SpawnCars());
        CurrentCameraView = m_EnableCarDashboardCamera ? m_CarCamera : m_IntersectionCamera;
        m_CarCamera.SetActive(m_EnableCarDashboardCamera);
        m_IntersectionCamera.SetActive(!m_EnableCarDashboardCamera);

        _drivingLogMetricDefinition = DatasetCapture.RegisterMetricDefinition("Driving log", "", Guid.Parse(drivingInfoMetricId));
        
        _carPathFollows.AddRange(FindObjectsOfType<PathFollow>());
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
            _carPathFollows.Add(pathFollow);

            if (spawnPoint == 0)
                yield return new WaitForSeconds(50.0f);
        }

    }

    public void Update()
    {
        var drivingInfos = _carPathFollows.Select(p => new DrivingInfo
        {
            position = p.transform.position,
            speed = p.currentSpeed,
            steer = p.steer
        }).ToArray();
        DatasetCapture.ReportMetric(_drivingLogMetricDefinition, drivingInfos);
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
            manager.m_CarCamera.SetActive(true);
            manager.m_IntersectionCamera.SetActive(false);
        }
        else
        {
            manager.m_CarCamera.SetActive(false);
            manager.m_IntersectionCamera.SetActive(true);
        }
    }
}
#endif
