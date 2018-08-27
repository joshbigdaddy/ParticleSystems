using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSimulator : MonoBehaviour {

    public int radius;
    public GameObject emitter;
    public int numberOfEmitters;
    public Vector3 center;
	// Use this for initialization
	void Start () {
        float angle = 0f;
	    for(int i=0;i< numberOfEmitters; i++)
        {

            Vector3 position = new Vector3(Mathf.Cos(angle)*radius,0, Mathf.Sin(angle)*radius);

            GameObject fountain = Instantiate(emitter, position, Quaternion.identity) as GameObject;
            angle += (float) Mathf.Deg2Rad * (360 /(float) numberOfEmitters);

        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
