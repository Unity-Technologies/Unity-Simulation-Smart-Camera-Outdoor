using UnityEngine;
using System.IO;

public class PositionTracker : MonoBehaviour
{

    float timeSinceLastCapture = 0;
    string[] positionsRotations = null;
    int curPos = 0;
    string filePath;

    Vector3 targetPosition;
    Quaternion targetRotation;

    public bool capturing = false;
    public int captureInterval = 1;
    public float speed = 0.1f;
    public string fileName = "pos.txt";



    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.streamingAssetsPath + "/" + fileName;

        if (capturing)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        else
        {
            positionsRotations = File.ReadAllLines(filePath);
        }
    }


    Vector3 stringToPosition(string inputString)
    {
       string[] pos = inputString.Substring(1, inputString.Length - 2).Split(',');
       return new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
    }

    Quaternion stringToRotation(string inputString)
    {
        string[] rot = inputString.Substring(1, inputString.Length - 2).Split(',');
        return new Quaternion(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]));
    }


    void Update()
    {
        timeSinceLastCapture += Time.deltaTime;

        if (timeSinceLastCapture >= captureInterval)
        {
            // Output position and rotation to file if capturing
            if (capturing)
            {
                string pos = transform.position.ToString();
                string rot = transform.rotation.ToString();
                string outputLine = pos + ':' + rot + '\n';
                File.AppendAllText(filePath, outputLine);
            }
            timeSinceLastCapture = 0.0f;
        }

        if (!capturing)
        {

            if (curPos == 0 && positionsRotations != null)
            {
                string[] posRot = positionsRotations[curPos].Split(':');

                targetPosition = stringToPosition(posRot[0]);
                targetRotation = stringToRotation(posRot[1]);
            }
            
            float dist = Vector3.Distance(transform.position, targetPosition);
            // 1.525 is the distanct that existed even when
            // transform.position == targetPosition should have evaluated as true.
            // Ex. (1.2, 2.1, 3.4) == (1.2, 2.1, 3.4) -> False
            if ((dist < 1.526 || transform.position == targetPosition) && positionsRotations != null)
            {
                // Loop through the positions indefinitely
                if (curPos == positionsRotations.Length)
                    curPos = 0;
                else
                    curPos++;

                string[] posRot = positionsRotations[curPos].Split(':');

                targetPosition = stringToPosition(posRot[0]);
                targetRotation = stringToRotation(posRot[1]);
            }
    
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 5);
        }
    }
}
