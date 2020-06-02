using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectMovementManager : MonoBehaviour
{
    public bool raycastPosition = false;
    public List<Vector3> controlPoints = new List<Vector3>();


    public float resolution = 0.1f;
    public float length = 0;
    private bool isLooping = true;


    public bool lengthGizmoCal = true;
    public float lengthGizmo = 0;


    public List<Vector3> GetPointList()
    {
        length = 0;
        List<Vector3> positions = new List<Vector3>();
        if (controlPoints.Count > 2)
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                if ((i == 0 || i == controlPoints.Count - 2 || i == controlPoints.Count - 1) && !isLooping)
                {
                    continue;
                }

                positions.AddRange(GenerateCatmullRomSpline(i));
            }
        }

        if (raycastPosition)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Ray ray = new Ray(positions[i] + Vector3.up , Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                    positions[i] = hit.point;
            }

        }

        return positions;
    }

    List<Vector3> GenerateCatmullRomSpline(int pos)
    {

        List<Vector3> positions = new List<Vector3>();
        float splineLength = 0;
        Random.InitState(111);

        Vector3 p0 = controlPoints[ClampListPos(pos - 1)];
        Vector3 p1 = controlPoints[pos];
        Vector3 p2 = controlPoints[ClampListPos(pos + 1)];
        Vector3 p3 = controlPoints[ClampListPos(pos + 2)];

        Vector3 lastPos = p1;



        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            float t = i * resolution;

            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            positions.Add(newPos);


            splineLength += Vector3.Distance(lastPos, newPos);

            lastPos = newPos;
        }
        length += splineLength;
        return positions;
    }

    public void RemovePoint(int i)
    {
        if (i < controlPoints.Count)
        {
            controlPoints.RemoveAt(i);
        }
    }

    public void RemovePoints()
    {

        controlPoints.Clear();

    }

    public void AddPoint(Vector4 position)
    {
        controlPoints.Add(position);
    }

    public void AddPointAfter(int i)
    {
        Vector3 position = controlPoints[i];
        if (i < controlPoints.Count - 1 && controlPoints.Count > i + 1)
        {
            Vector3 positionSecond = controlPoints[i + 1];
            if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                position = (position + positionSecond) * 0.5f;
            else
                position.x += 1;
        }
        else if (controlPoints.Count > 1 && i == controlPoints.Count - 1)
        {
            Vector3 positionSecond = controlPoints[i - 1];
            if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                position = position + (position - positionSecond);
            else
                position.x += 1;
        }
        else
        {
            position.x += 1;
        }
        controlPoints.Insert(i + 1, position);
    }

    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = controlPoints.Count - 1;
        }

        if (pos > controlPoints.Count)
        {
            pos = 1;
        }
        else if (pos > controlPoints.Count - 1)
        {
            pos = 0;
        }

        return pos;
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {

        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }


    public void OnDrawGizmosSelected()
    {
        lengthGizmo = 0;

        Gizmos.color = Color.red;
        foreach (var item in controlPoints)
        {
            Gizmos.DrawSphere(item, 0.2f);
        }

        Gizmos.color = Color.red;

        if (controlPoints.Count < 2)
            return;

        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPoints.Count; i++)
        {
            //Cant draw between the endpoints
            //Neither do we need to draw from the second to the last endpoint
            //...if we are not making a looping line
            if ((i == 0 || i == controlPoints.Count - 2 || i == controlPoints.Count - 1) && !isLooping)
            {
                continue;
            }

            DisplayCatmullRomSpline(i);
        }
    }

    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int pos)
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = controlPoints[ClampListPos(pos - 1)];
        Vector3 p1 = controlPoints[pos];
        Vector3 p2 = controlPoints[ClampListPos(pos + 1)];
        Vector3 p3 = controlPoints[ClampListPos(pos + 2)];

        //The start position of the line
        Vector3 lastPos = p1;


        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            //Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);

            if (lengthGizmoCal)
                lengthGizmo += Vector3.Distance(newPos, lastPos);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }
}