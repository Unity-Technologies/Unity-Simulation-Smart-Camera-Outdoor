using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForestManager : MonoBehaviour {

    public Vector2 rangeRotation = new Vector2(0, 180);
    public Vector2 rangeScale = new Vector2(0.7f, 1.3f);
    public LayerMask mask;

    public bool randomize = false;

	// Use this for initialization
	void Start ()
    {
        RandomizeTrees();
    }

    private void RandomizeTrees()
    {
        foreach (Transform child in transform)
        {
            child.Rotate(0, Random.Range(rangeRotation.x, rangeRotation.y), 0, Space.Self);
            child.localScale = Vector3.one * Random.Range(rangeScale.x, rangeScale.y);
            RaycastHit hit;
            if (Physics.Raycast(child.position + Vector3.up * 10000, Vector3.down, out hit, 100000, mask))
            {
                child.position = hit.point;
            }
        }
    }

    // Update is called once per frame
    void Update () {
		if(randomize)
        {
            randomize = false;
            RandomizeTrees();
        }
	}
}
