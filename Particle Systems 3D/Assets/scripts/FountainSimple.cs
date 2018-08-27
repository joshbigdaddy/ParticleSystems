using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



#if UNITY_EDITOR
[System.Serializable]
#endif
public class FountainSimple : MonoBehaviour
{

    [Range(0, 1)]
    public float randomness = 0;
    [Range(1, 10)]
    public float lifetime = 2;
    [Range(0f, 1f)]
    public float waitTime = 0.3f;
    public float velocity;
    private Vector3 vectorEmitter = new Vector3(0, 0, 0);

    private GameObject emissionObject;
    public Vector3 origin;
    public GameObject[] particles;
    public Material[] colors;

    public class Particle
    {
        public GameObject particle;
        public Vector3 velocity;
        public Vector3 gravity;
        public int hashId = 0;
        public float mass = 1f;
        public Vector3 aceleration;
        public Particle()
        {

            particle = null;
            gravity = new Vector3(0, 0, 0);
            velocity = new Vector3(0, 0, 0);
            aceleration = new Vector3(0, 0, 0);
        }
    }
    // Use this for initialization
    void Start()
    {   emissionObject=this.gameObject;
        vectorEmitter = emissionObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            
                StartCoroutine(GenerateParticlesStreamRGBody());
            
        }
    }


    IEnumerator GenerateParticlesStreamRGBody()
    {

        while (true)
        {
            Particle part = new Particle();
            part.particle = Instantiate(particles[Random.Range(0,particles.Length)], vectorEmitter, Quaternion.identity) as GameObject;
            Rigidbody rigidbody = part.particle.GetComponent<Rigidbody>();
            rigidbody.velocity= (origin - emissionObject.gameObject.transform.position).normalized * velocity + new Vector3(0, velocity, 0);
            print(rigidbody.velocity);
            part.particle.GetComponent<MeshRenderer>().material = colors[Random.Range(0, colors.Length)];
            StartCoroutine(Fade(part));
            yield return new WaitForSeconds(0.09f);
        }

    }


    IEnumerator Fade(Particle obj)
    {

        float f = lifetime + Random.Range(0, lifetime * randomness);
        Destroy(obj.particle, f);

        yield return null;


    }
}