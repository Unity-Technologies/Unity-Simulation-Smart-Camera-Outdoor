using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Unity.Simulation.Logger;

public class PathFollow : MonoBehaviour
{
    [Header("Car Configuration")]
    public Transform path;
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;
    public float maxSteerAngle = 45.0f;
    public int waitAtPosition = -1;


    [Header("Sensor")] 
    public float sensorLength = 0.5f;
    public float frontSensorPosition = 0.5f;

    [Header("Path Info")] 
    public int startingPoint;
    
    
    private List<Transform> _waypointNodes;
    private int             _currentPosition = 0;
    private bool _greenLight = true;
    private bool _isBraking;
    private bool _forceBrake;


    private Logger _simulationLogger;


    struct DrivingLog
    {
        public Vector3 position;
        public float steer;
        public double speed;

    }

    private DrivingLog _log;

    private bool _logsEnabled;
    // Start is called before the first frame update
    void Start()
    {
        var childernNodes = path.GetComponentsInChildren<Transform>();
        _waypointNodes = new List<Transform>(childernNodes.Length);
        _currentPosition = startingPoint;

        foreach (var n in childernNodes)
        {
            if (n.position != path.transform.position)
                _waypointNodes.Add(n);
        }

        _logsEnabled = SimulationOptions.CaptureDrivingLogs;

        if (_logsEnabled)
        {
            _simulationLogger = new Logger(transform.parent.name + "_DrivingLog");
            _log = new DrivingLog();
        }
    }

    private void Update()
    {
        if (waitAtPosition != -1 && waitAtPosition == _currentPosition && _greenLight)
        {
            _greenLight = false;
            _forceBrake = true;
            StartCoroutine(WaitAtPosition());
        }

    }

    IEnumerator WaitAtPosition()
    {
        yield return new WaitForSeconds(2.5f);
        _forceBrake = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplySteer();
        Drive();
        CheckWayPointDistance();
        if (!_isBraking)
            StartCoroutine(ApplyBraking(5.0f));
        
        if (_logsEnabled)
            _simulationLogger.Log(_log);
        
    }

    void ApplySteer()
    {
        var relativePos = transform.InverseTransformPoint(_waypointNodes[_currentPosition].position);
        var steer = (relativePos.x / relativePos.magnitude) * maxSteerAngle;
        frontLeft.steerAngle = steer;
        frontRight.steerAngle = steer;
        if (_logsEnabled)
        {
            _log.steer = steer;
            _log.position = transform.position;   
        }
    }

    IEnumerator ApplyBraking(float brakeTorque)
    {
        RaycastHit hit;
        var start = new Vector3(transform.position.x, _waypointNodes[_currentPosition].position.y + 0.5f, transform.position.z);
        var direction = _waypointNodes[_currentPosition].position - start;
        Debug.DrawLine(start, _waypointNodes[_currentPosition].position, Color.red);
        if (Physics.Raycast( start, direction, out hit, 4.5f))
        {
            
            if (hit.collider.tag == "car")
            {
                rearLeft.brakeTorque = brakeTorque;
                rearRight.brakeTorque = brakeTorque;
                frontLeft.motorTorque = 0;
                frontRight.motorTorque = 0;
                _isBraking = true;
                yield return new WaitForSeconds(1.0f);
                rearRight.brakeTorque = 0;
                rearLeft.brakeTorque = 0;
                rearLeft.motorTorque = 10.0f;
                rearRight.motorTorque = 10.0f;
                _isBraking = false;
            }
        }
    }

    void Drive()
    {
        var currentSpeed = 2 * Math.PI * frontLeft.radius * frontLeft.rpm * 60 / 1000;
        _log.speed = currentSpeed;
        if (currentSpeed < 1.0f && !_isBraking)
        {
            frontLeft.motorTorque = 20.0f;
            frontRight.motorTorque = 20.0f;
        }
    }

    void CheckWayPointDistance()
    {
        if ((transform.position - _waypointNodes[_currentPosition].position).sqrMagnitude <= 1.0f)
        {
            if (_currentPosition == _waypointNodes.Count - 1)
            {
                _currentPosition = 0;   
            }
            else
            {
                _currentPosition++;   
            }
        }
    }
}
