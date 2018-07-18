using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaGenerator : MonoBehaviour {

    

    [Range(1,5)]
    public int length = 1;
    [Range(1, 5)]
    public int depth = 1;

    // Use this for initialization
    void Start () {
        //WaveGenerator.Instance.Test();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
