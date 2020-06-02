using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("Movement")]
    public GameObject objetToMove;
    public ObjectMovementManager objectMovementManager;

    [Header("Parameters")]
    public float speed = 0.5f;
    public float speedRandom = 0f;
    public float speedWheelsMultiplier = 1f;
    public float rotationSpeed = 5;

    public float position = 0.0f;
    public bool stopRotation = false;

    [Header("Random")]
    public float randomRange = 0;
    public float randomPos = 0;

    [Header("Wheels")]
    public Transform[] wheels;
    public Vector3 wheelRotationAxis = new Vector3(0, 0, 1);


    [Header("Trailer")]
    public Transform trailer;
    public float lerpSpeedTrailer = 1;
    public int rotationsCheckNumber = 5;

    [Header("Additional settings")]
    public bool raycastPosition = false;
    public bool debug = false;


    private List<float> trailerRotations = new List<float>();

    private List<Vector3> movementPoints = new List<Vector3>();

    private int currentPart = 0;

    private Vector3 newPosition;
    private Vector3 oldPosition;

    public float sideOffset = 0;

    private static List<float> randomList = new List<float>();


    public static float GetRandomFromList()
    {

        if (randomList == null)
        {
            randomList = new List<float>();
        }

        if (randomList.Count <= 0)
        {

            Random.InitState(System.DateTime.Now.Millisecond);
            for (int i = 0; i < 2000; i++)
            {
                randomList.Add(Random.value);
            }
        }

        float value = randomList[0];
        randomList.RemoveAt(0);
        return value;
    }

    new Rigidbody rigidbody;
    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }

        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
        }

        movementPoints = objectMovementManager.GetPointList();

        if (randomRange != 0)
            sideOffset = (randomRange * GetRandomFromList() * 2 - randomRange) * 0.5f;

        //randomList.RemoveAt(0);

        if (speed < 0)
        {
            speed = -speed;
            movementPoints.Reverse();
        }

        speed += (speedRandom * GetRandomFromList() * 2 - speedRandom) * 0;

        position += GetRandomFromList() * randomPos * speed;

    }



    // Update is called once per frame
    void UpdatePosition()
    {
        oldPosition = newPosition;

        float length = Vector3.Distance(movementPoints[currentPart], movementPoints[ClampListPos(currentPart + 1, movementPoints.Count)]);

        position += speed * Time.fixedDeltaTime;

        while (position / (float)length > 0.9999f)
        {
            currentPart = ClampListPos(currentPart + 1, movementPoints.Count);
            position = position - length;
            length = Vector3.Distance(movementPoints[currentPart], movementPoints[ClampListPos(currentPart + 1, movementPoints.Count)]);
        }


        newPosition = Vector3.Lerp(movementPoints[ClampListPos(currentPart, movementPoints.Count)], movementPoints[ClampListPos(currentPart + 1, movementPoints.Count)], position / (float)length);
        // newPosition.y = 341.478f;

        objetToMove.transform.rotation = Quaternion.Slerp(objetToMove.transform.rotation, Quaternion.LookRotation(newPosition - oldPosition), rotationSpeed * Time.fixedDeltaTime);

        if (stopRotation)
        {
            Vector3 euler = objetToMove.transform.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            objetToMove.transform.eulerAngles = euler;
        }

        if (rigidbody == null)
        {
            if (raycastPosition)
            {
                Ray ray = new Ray(newPosition + Vector3.up * 100, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                    newPosition.y = hit.point.y;
            }

            objetToMove.transform.position = newPosition;
            objetToMove.transform.position += objetToMove.transform.right * sideOffset;
        }



        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Rotate(wheelRotationAxis * (-speedWheelsMultiplier * speed * Time.fixedDeltaTime));
        }

        if (trailer != null)
        {
            trailerRotations.Add(objetToMove.transform.eulerAngles.y);


            if (trailerRotations.Count > rotationsCheckNumber)
                trailerRotations.RemoveAt(0);


            trailer.localRotation = Quaternion.Slerp(trailer.localRotation, Quaternion.Euler(0, 0, trailerRotations.Average() - objetToMove.transform.eulerAngles.y), lerpSpeedTrailer * Time.fixedDeltaTime);


        }

    }

    Quaternion AverageQuaternion(List<Quaternion> qArray)
    {
        Quaternion qAvg = qArray[0];
        float weight;
        for (int i = 1; i < qArray.Count; i++)
        {
            weight = 1.0f / (float)(i + 1);
            qAvg = Quaternion.Slerp(qAvg, qArray[i], weight);
        }
        return qAvg;
    }

    private void FixedUpdate()
    {
        UpdatePosition();
        if (rigidbody != null)
        {
            rigidbody.MovePosition(newPosition + objetToMove.transform.right * sideOffset);
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (!debug)
            return;
        if (!objetToMove)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(oldPosition, newPosition);
        Gizmos.DrawSphere(oldPosition, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(objetToMove.transform.position, objetToMove.transform.position + (newPosition - objetToMove.transform.position).normalized * 100);
        Gizmos.DrawSphere(newPosition, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(objetToMove.transform.position, objetToMove.transform.position + objetToMove.transform.forward * 100);

        if (movementPoints.Count > 1)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(movementPoints[ClampListPos(currentPart, movementPoints.Count)], 0.1f);
            Gizmos.DrawSphere(movementPoints[ClampListPos(currentPart + 1, movementPoints.Count)], 0.1f);
        }

        // if (objectMovementManager)
        //     objectMovementManager.OnDrawGizmosSelected();

        if (movementPoints != null)
            for (int i = 0; i < movementPoints.Count; i++)
            {
                Gizmos.DrawSphere(movementPoints[i], 0.1f);
                Gizmos.DrawLine(movementPoints[i], movementPoints[(i + 1) % movementPoints.Count]);
            }

    }

    int ClampListPos(int pos, int count)
    {
        if (pos < 0)
        {
            pos = count - 1;
        }

        if (pos > count)
        {
            pos = 1;
        }
        else if (pos > count - 1)
        {
            pos = 0;
        }

        return pos;
    }
}
