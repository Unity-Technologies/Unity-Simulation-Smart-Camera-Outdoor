using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Simulation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Perception.GroundTruth;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera m_Camera;
    public Camera m_CameraSemantic;
    public Camera m_CameraDepth;

    [Header("Camera Hang Points")]
    public List<Transform> m_CameraPoints;
    public List<Transform> m_CameraSemanticPoints;
    
    [Header("Additional Traffic Car Spawning")]
    public GameObject       m_CarPrefab;
    public int              m_NumberOfCars;
    public List<SpawnPoint> m_SpawnPoints;
    public Material[]       m_CarMaterials;

    const int MaxCarsAllowed = 10;
    
    int _camera_pos_id = 0;

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
        SetupSemanticSegmentation();
        if (Configuration.Instance.IsSimulationRunningInCloud())
        {
            SetupDepthGrab();
            if (SimulationOptions.CameraViewToCapture == "Car")
                SwitchCameraView();
        }
    }

    private void SetupSemanticSegmentation()
    {
        var applySegShader = GetComponent<ApplySemanticSegmentationShader>();
        applySegShader.m_SemanticSegmentationCamera = m_CameraSemantic;
        applySegShader.ApplySemanticSegmentation();
    }

    private void SetupDepthGrab()
    {
        if (SimulationOptions.CaptureDepth || !Configuration.Instance.IsSimulationRunningInCloud())
        {
            var depthGrab = GetComponent<DepthGrab>();
            Debug.Assert(depthGrab != null, "Depth Grab component is not added");
            depthGrab.enabled = !Configuration.Instance.IsSimulationRunningInCloud() || SimulationOptions.CaptureDepth;
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
        _camera_pos_id = (_camera_pos_id+1) % m_CameraPoints.Count;
        m_Camera.transform.SetParent(m_CameraPoints[_camera_pos_id], false);
        m_CameraSemantic.transform.SetParent(m_CameraSemanticPoints[_camera_pos_id], false);
        m_CameraDepth.transform.SetParent(m_CameraPoints[_camera_pos_id], false);

        // point 0 is at intersection
        if (_camera_pos_id == 0) {
            DisableDepthCapture();
        }
        else {
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
