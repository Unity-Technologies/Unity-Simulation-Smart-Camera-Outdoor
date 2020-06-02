using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversePointsOrder : MonoBehaviour {

    public ObjectMovementManager source;
    public ObjectMovementManager destination;

    public void ReverseOrder()
    {
        destination.controlPoints.Clear();
        Debug.Log(destination.controlPoints.Count);
        Debug.Log(source.controlPoints.Count);

        for (int i = source.controlPoints.Count-1; i > 0; i--)
        {
            Debug.Log(source.controlPoints[i]);
            destination.controlPoints.Add(source.controlPoints[i]);
        }
        Debug.Log(destination.controlPoints.Count);
    }
}
