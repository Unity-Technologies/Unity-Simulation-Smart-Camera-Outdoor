using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineAtStart : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        Debug.Log(name);
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(false);
            //Destroy(item.gameObject);
        }
    }


}
