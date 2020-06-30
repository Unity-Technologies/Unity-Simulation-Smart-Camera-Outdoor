﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Simulation;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
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
        CurrentCameraView = m_IntersectionCamera;
        //EnableIntersectionCameraView();
        if (Configuration.Instance.IsSimulationRunningInCloud())
        {
            SetupDepthGrab();
            if (SimulationOptions.CameraViewToCapture == "Car")
                SwitchCameraView();
        }
    }

    private void Update()
    {
    }

    private void EnableIntersectionCameraView()
    {
        
        Debug.Assert(m_IntersectionCamera.GetComponent<Camera>() != null, "No Camera component added");
        var perceptionCam = m_IntersectionCamera.GetComponent<PerceptionCamera>();
        if (perceptionCam == null)
        {
            var comp = m_IntersectionCamera.AddComponent<PerceptionCamera>();
            var currentPerceptionCam = CurrentCameraView.GetComponent<PerceptionCamera>();
            comp.period = currentPerceptionCam.period;
            comp.captureRgbImages = currentPerceptionCam.captureRgbImages;
            comp.produceSegmentationImages = currentPerceptionCam.produceSegmentationImages;
            comp.LabelingConfiguration = currentPerceptionCam.LabelingConfiguration;
            Destroy(CurrentCameraView.GetComponent<PerceptionCamera>());
        }
        
        m_CarCamera.SetActive(false);
        m_IntersectionCamera.SetActive(true);
        CurrentCameraView = m_IntersectionCamera;
//        m_IntersectionCameraSemantic.SetActive(true);
//        m_IntersectionMainCamera.SetActive(true);
//        m_CarCameraSemantic.SetActive(false);
//        m_CarMainCamera.SetActive(false);
//        GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera = GameObject.Find("IntersectionSemanticSegCamera").GetComponent<Camera>();
//        GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
    }

    private void EnableCarDashboardCamera()
    {
//        Debug.Assert(m_CarCamera.GetComponent<Camera>() != null, "No Camera component added");
//        var perceptionCam = m_CarCamera.GetComponent<PerceptionCamera>();
//        if (perceptionCam == null)
//        {
//            var comp = m_CarCamera.AddComponent<PerceptionCamera>();
//            var currentPerceptionCam = CurrentCameraView.GetComponent<PerceptionCamera>();
//            comp.period = currentPerceptionCam.period;
//            comp.captureRgbImages = currentPerceptionCam.captureRgbImages;
//            comp.produceSegmentationImages = currentPerceptionCam.produceSegmentationImages;
//            comp.LabelingConfiguration = currentPerceptionCam.LabelingConfiguration;
//            Destroy(CurrentCameraView.GetComponent<PerceptionCamera>());
//        }
        
        m_IntersectionCamera.SetActive(false);
        CurrentCameraView = m_CarCamera;
        m_CarCamera.SetActive(true);
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
//        if (m_CarMainCamera.active)
//        {
//            m_CarMainCamera.SetActive(false);
//            m_CarCameraSemantic.SetActive(false);
//            m_IntersectionMainCamera.SetActive(true);
//            m_IntersectionCameraSemantic.SetActive(true);
//            GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera = GameObject.Find("IntersectionSemanticSegCamera").GetComponent<Camera>();
//            GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
//            var cameraGrab = GetComponent<CameraGrab>();
//            cameraGrab._cameraSources = m_IntersectionMainCamera.transform.GetComponentsInChildren<Camera>().Where(c=>c.gameObject.name != "_DepthCam").ToArray();
//            DisableDepthCapture();
//        }
//        else
//        {
//            m_IntersectionMainCamera.SetActive(false);
//            m_IntersectionCameraSemantic.SetActive(false);
//            m_CarMainCamera.SetActive(true);
//            m_CarCameraSemantic.SetActive(true);
//            GetComponent<ApplySemanticSegmentationShader>().m_SemanticSegmentationCamera =  GameObject.Find("_SegmentCam").GetComponent<Camera>();;
//            GetComponent<ApplySemanticSegmentationShader>().ApplySemanticSegmentation();
//            var cameraGrab = GetComponent<CameraGrab>();
//            cameraGrab._cameraSources = m_CarMainCamera.transform.GetComponentsInChildren<Camera>().Where(c=>c.gameObject.name != "_DepthCam").ToArray();
//            SetupDepthGrab();
//        }

        if (m_CarCamera.active)
        {
            EnableIntersectionCameraView();   
        }
        else
        {
            EnableCarDashboardCamera();
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

//            var semanticSeg = GetComponent<ApplySemanticSegmentationShader>();
//            if (semanticSeg)
//                semanticSeg.ApplySemanticSegmentationForObject(car);

            AddLabelingToGameObject(car);
            
            if (spawnPoint == 0)
                yield return new WaitForSeconds(50.0f);
        }

    }


    public void AddLabelingToGameObject(GameObject go)
    {
        Debug.Assert(!String.IsNullOrEmpty(go.tag), "The GameObject is not tagged");
        var pcam = CurrentCameraView.GetComponent<PerceptionCamera>();
        var lastLabel = pcam.LabelingConfiguration.LabelEntries.Last();
        pcam.LabelingConfiguration.LabelEntries.Add(new LabelEntry() { id = lastLabel.id + 1, label = go.tag, value = lastLabel.value + 1000});
    }
}
