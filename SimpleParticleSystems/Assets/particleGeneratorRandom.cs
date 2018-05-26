using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleGeneratorRandom : MonoBehaviour {

    public float lifetime = 1;
    
    public float forceRanges = 500f;
    [Range(0f,1f)]
    public float waitTime = 0.3f;
    [Range(1,1000)]
    public int numberOfParticles = 100;
    [Range(0,360)]
    public float angleRange = 180;
    private Vector3 vectorEmitter = new Vector3(0, 0, 0);
    public GameObject particle;
    public bool isExplosionDivided = false;
    
    // Use this for initialization
    void Start () {
       
    vectorEmitter.x = GameObject.Find("ParticleGenerator").transform.position.x;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
           StartCoroutine(GenerateParticlesStream(numberOfParticles));

        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter)&& !isExplosionDivided)
        {
            StartCoroutine(GenerateParticlesExplosionNotDivided(numberOfParticles));

        }else if (Input.GetKeyDown(KeyCode.KeypadEnter) && isExplosionDivided)
        {
            StartCoroutine(GenerateParticlesExplosionDivided(numberOfParticles));
        }
    }

    IEnumerator GenerateParticlesStream( int number)
    {
        int particlesLaunched = 0;
        while (number>particlesLaunched)
        {
            GameObject obj = Instantiate(particle, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            Rigidbody2D rigidbody = obj.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(new Vector2(Random.Range(-forceRanges, forceRanges), Random.Range(0, forceRanges)));
            StartCoroutine(Fade(obj.GetComponent<SpriteRenderer>(), obj));
            particlesLaunched++;
            yield return null;
        }
        
    }

    IEnumerator GenerateParticlesExplosionNotDivided(int number)
    {
        int particlesLaunched = 0;
        while (number > particlesLaunched)
        {
            
                GameObject obj = Instantiate(particle, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                Rigidbody2D rigidbody = obj.GetComponent<Rigidbody2D>();
                rigidbody.AddForce(new Vector2(Random.Range(-forceRanges, forceRanges), Random.Range(0, forceRanges)));
                StartCoroutine(Fade(obj.GetComponent<SpriteRenderer>(), obj));
                particlesLaunched++;
            
        }
        yield return null;
    }

    IEnumerator GenerateParticlesExplosionDivided(int number)
    {
        int particlesLaunched = 0;
        int times = number / 100;
        for (int f = 1; f <= times; f ++) {
            particlesLaunched = 0;
            while (number/times > particlesLaunched)
            {
                
                GameObject obj = Instantiate(particle, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                Rigidbody2D rigidbody = obj.GetComponent<Rigidbody2D>();
                rigidbody.AddForce(new Vector2(Random.Range(-forceRanges, forceRanges), Random.Range(0, forceRanges)));
                StartCoroutine(Fade(obj.GetComponent<SpriteRenderer>(), obj));
                particlesLaunched++;
                
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
    IEnumerator Fade(Renderer renderer,GameObject obj)
    {
        for (float f = lifetime; f >= 0; f -= 0.1f)
        {
            Color c = renderer.material.color;
            c.a = f;
            renderer.material.color = c;            
            yield return null;
        }
        Destroy(obj);
    }
}
