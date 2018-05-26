using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleGenerator : MonoBehaviour {

    private List<Particle> particles;
    public int numberOfParticles=100;
    public GameObject emissionObject;
    public GameObject prefabParticle;

    public class Particle
    {
        public float lifeTime = 0;
        public GameObject particle;
        public Vector3 velocity;
        public Vector3 gravity;
    }
	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GenerateParticlesStream(numberOfParticles));

        }
    }

    IEnumerator GenerateParticlesStream(int number)
    {

        int launched = 0;
        while (number > launched)
        {

            Particle particle = new Particle();

            particle.particle = Instantiate(prefabParticle, emissionObject.transform.position, Quaternion.identity) as GameObject;

            launched++;
            yield return null;
        }

    }
}
