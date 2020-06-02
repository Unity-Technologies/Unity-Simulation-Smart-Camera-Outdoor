using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Simulation;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{
    
    [Header("Semantic Segmentation View")]
    public GameObject m_CarCameraSemantic;
    public GameObject m_IntersectionCameraSemantic;
    
    [Header("Main Camera View")]
    public GameObject m_CarMainCamera;
    public GameObject m_IntersectionMainCamera;
    
    [Header("Additional Traffic Car Spawning")]
    public GameObject       m_CarPrefab;
    public int              m_NumberOfCars;
    public List<SpawnPoint> m_SpawnPoints;
    public Material[]       m_CarMaterials;

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
        EnableIntersectionCameraView();
        if (Configuration.Instance.IsSimulationRunningInCloud())
        {
            SetupDepthGrab();
            if (SimulationOptions.CameraViewToCapture == "Car")
                SwitchCameraView();
        }
    }

    private void EnableIntersectionCameraView()
    {
        m_IntersectionCameraSemantic.SetActive(true);
        m_IntersectionMainCamera.SetActive(true);
        m_CarCameraSemantic.SetActive(false);
        m_CarMainCamera.SetActive(false);
        GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera = GameObject.Find("IntersectionSemanticSegCamera").GetComponent<Camera>();
        GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
    }

    private void SetupDepthGrab()
    {
        if (SimulationOptions.CaptureDepth || !Configuration.Instance.IsSimulationRunningInCloud())
        {
            var usimCapture = GameObject.Find("USimCaptureDemo");
            if (usimCapture != null)
            {
                usimCapture.SetActive(true);
                var depthGrab = usimCapture.GetComponent<DepthGrab>();
                Debug.Assert(depthGrab != null, "Depth Grab component is not added");
                depthGrab.enabled = !Configuration.Instance.IsSimulationRunningInCloud() || SimulationOptions.CaptureDepth;
                var cameras =  m_CarMainCamera.transform.GetComponentsInChildren<Camera>();
                foreach (var cam in cameras)
                {
                    if (cam.CompareTag("DepthCamera"))
                        depthGrab._camera = cam;
                }
            }
        }
    }

    private void DisableDepthCapture()
    {
        var depthGrabComponent = GetComponent<DepthGrab>();
        if (depthGrabComponent != null)
            depthGrabComponent.enabled = false;
    }

    /// <summary>
    /// Switch the camera view between intersection camera view and Car first person view
    /// </summary>
    public void SwitchCameraView()
    {
        if (m_CarMainCamera.active)
        {
            m_CarMainCamera.SetActive(false);
            m_CarCameraSemantic.SetActive(false);
            m_IntersectionMainCamera.SetActive(true);
            m_IntersectionCameraSemantic.SetActive(true);
            GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera = GameObject.Find("IntersectionSemanticSegCamera").GetComponent<Camera>();
            GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
            var cameraGrab = GetComponent<CameraGrab>();
            cameraGrab._cameraSources = m_IntersectionMainCamera.transform.GetComponentsInChildren<Camera>().Where(c=>c.gameObject.name != "_DepthCam").ToArray();
            DisableDepthCapture();
        }
        else
        {
            m_IntersectionMainCamera.SetActive(false);
            m_IntersectionCameraSemantic.SetActive(false);
            m_CarMainCamera.SetActive(true);
            m_CarCameraSemantic.SetActive(true);
            GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera =  GameObject.Find("_SegmentCam").GetComponent<Camera>();;
            GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
            var cameraGrab = GetComponent<CameraGrab>();
            cameraGrab._cameraSources = m_CarMainCamera.transform.GetComponentsInChildren<Camera>().Where(c=>c.gameObject.name != "_DepthCam").ToArray();
            SetupDepthGrab();
        }
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

            var semanticSeg = GetComponent<ApplySemanticSegmentationShader>();
            if (semanticSeg)
                semanticSeg.ApplySemanticSegmentationForObject(car);
        
            if (spawnPoint == 0)
                yield return new WaitForSeconds(50.0f);
        }

    }
}
