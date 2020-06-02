using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeAnimation : MonoBehaviour {

    public bool cycling;

    public Animator[] animators;
	
	// Update is called once per frame
	void Update () {
	
            foreach (var item in animators)
            {
                item.SetBool("cycling", cycling);
            }      

      
    }
}
