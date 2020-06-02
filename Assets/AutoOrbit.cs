using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Orbit))]
public class AutoOrbit : MonoBehaviour
{
    Orbit __orbit;
    Orbit orbit { get { return __orbit ?? (__orbit = GetComponent<Orbit>()); } }

    public float delaySeconds = 1;
    public float speed = 1;

    bool originalSnap;
    private void OnEnable()
    {
        start = false;
        StartCoroutine(DelayedActivation());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    bool start = true;
    IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(delaySeconds);
        start = true;
    }

    private void Update()
    {
        if (start)
        {
            orbit.azimuth += speed * Time.deltaTime;
        }
    }
}