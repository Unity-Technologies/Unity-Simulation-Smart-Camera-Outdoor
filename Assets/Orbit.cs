using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Orbit : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;

    protected Vector3 targetPos
    {
        get { return (target == null ? Vector3.zero : target.position) + targetOffset; }
    }

    public float distance;
    public float elevation;
    public float azimuth;

    Transform oldTarget;
    Vector3 oldTargetOffset;
    float oldDistance;
    float oldElevation;
    float oldAzimuth;
    protected virtual void Update()
    {
        if (target != oldTarget || oldTargetOffset != targetOffset ||
            distance != oldDistance || elevation != oldElevation || azimuth != oldAzimuth)
        {
            elevation = Mathf.Clamp(elevation, -90, 90);
            while (azimuth < -180)
                azimuth += 360;
            while (azimuth > 180)
                azimuth -= 360;

            oldTarget = target;
            oldDistance = distance;
            oldElevation = elevation;
            oldAzimuth = azimuth;
            oldTargetOffset = targetOffset;

            Vector3 azymuthBase = Vector3.forward;
            transform.position = targetPos + azymuthBase * distance;
            transform.RotateAround(targetPos, Vector3.up, azimuth);

            Vector3 axis = Vector3.Cross(transform.position - targetPos, Vector3.up);
            transform.RotateAround(targetPos, axis, elevation);

            transform.LookAt(targetPos);
        }
    }
}
