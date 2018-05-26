using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
#if UNITY_EDITOR
[CustomEditor(typeof(FountainGenerator))]
public class MyScriptEditor : Editor
{
    override public void OnInspectorGUI()
    {
        var myScript = target as FountainGenerator;




        SerializedObject serializedObj = new SerializedObject(target);
        SerializedProperty lights = serializedObject.FindProperty("particleEvolution");

        serializedObj.Update();
        EditorGUILayout.HelpBox("Default Inspector", MessageType.None);
        DrawDefaultInspector();
        serializedObj.ApplyModifiedProperties();

        myScript.RigidBodySimulation = GUILayout.Toggle(myScript.RigidBodySimulation, "Activate Unity's Rigids Simulation");
        myScript.manualVelocities = GUILayout.Toggle(myScript.manualVelocities, "Activate Manual Input");

        if (myScript.RigidBodySimulation)
        {
        }
        else
        {
            myScript.gravity = EditorGUILayout.FloatField("Gravity:", myScript.gravity);
        }
        if (myScript.manualVelocities)
        {
            myScript.velocityOfXAxis = EditorGUILayout.FloatField("Velocity of X Axis field:", myScript.velocityOfXAxis);
            myScript.velocityOfYAxis = EditorGUILayout.FloatField("Velocity of Y Axis field:", myScript.velocityOfYAxis);
            myScript.velocityOfZAxis = EditorGUILayout.FloatField("Velocity of Z Axis field:", myScript.velocityOfZAxis);
        }
        else
        {
            myScript.velocity = EditorGUILayout.FloatField("Velocity:", myScript.velocity);
        }
    }

}
#endif

#if UNITY_EDITOR
[System.Serializable]
#endif
public class FountainGenerator : MonoBehaviour
{


    [Range(1, 10)]
    public float lifetime = 2;
    [Range(0f, 1f)]
    public float waitTime = 0.3f;
    [Range(0f, 1f)]
    public float randomness = 0.5f;
    [Range(1, 1000)]
    public int numberOfParticles = 100;

    [Range(0, 1)]
    public float elasticity = 1;

    [HideInInspector]
    public bool manualVelocities;
    [HideInInspector]
    public float velocity;
    [HideInInspector]
    public float velocityOfXAxis;
    [HideInInspector]
    public float velocityOfYAxis;
    [HideInInspector]
    public float velocityOfZAxis;

    private Vector3 vectorEmitter = new Vector3(0, 0, 0);

    [HideInInspector]
    public bool RigidBodySimulation = false;
    [HideInInspector]
    public float gravity = 1;
    [Range(1, 20)]
    public int numberOfCollisionsPermitted = 4;

    public bool collisionsOn = true;
    public GameObject emissionObject;
    public GameObject particle;
    public bool isRandomScale;
    public Sprite[] particleEvolution;

    [Range(100, 2000)]
    private int numberOfCellsInHash = 1000;
    private float h = 0.48f;
    private int id1 = 1;
   
    /**
     * This spatialHashCollisions is going to be the dictionary in which we are going to study the collisions
     * */
    private Hashtable spatialHashCollisions = new Hashtable();
    
    //This method will add to our HashTable a value

    public void addToHash(int position, Particle particle)
    {
        for (int i = 0; i < numberOfCellsInHash; i++)
        {
            if (spatialHashCollisions.ContainsKey(i))
            {

                List<Particle> list = (List<Particle>)spatialHashCollisions[i];
                list.Remove(particle);

                if (list.Count == 0)
                {
                    spatialHashCollisions.Remove(i);
                }
                else
                {
                    spatialHashCollisions[i] = list;
                }

            }
        }

        if (!spatialHashCollisions.Contains(position))
        {
            List<Particle> list = new List<Particle>();
            list.Add(particle);
            spatialHashCollisions.Add(position, list);
        }

        else
        {
            List<Particle> list = (List<Particle>)spatialHashCollisions[position];
            list.Remove(particle);
            list.Add(particle);
            spatialHashCollisions[position] = list;
        }

    }
    public List<int> findNeighbours(int hash, Vector3 position)
    {
        List<int> neighbours = new List<int>();

        neighbours.Add(hash);



        Vector3 neighbour1 = new Vector3(position.x + h, position.y + h, position.z);
        Vector3 neighbour2 = new Vector3(position.x + h, position.y - h, position.z);
        Vector3 neighbour3 = new Vector3(position.x + h, position.y, position.z);
        Vector3 neighbour4 = new Vector3(position.x, position.y + h, position.z);
        Vector3 neighbour5 = new Vector3(position.x, position.y - h, position.z);
        Vector3 neighbour6 = new Vector3(position.x - h, position.y + h, position.z);
        Vector3 neighbour7 = new Vector3(position.x - h, position.y - h, position.z);
        Vector3 neighbour8 = new Vector3(position.x - h, position.y, position.z);

        Vector3 neighbour9 = new Vector3(position.x + h, position.y + h, position.z + h);
        Vector3 neighbour10 = new Vector3(position.x + h, position.y - h, position.z + h);
        Vector3 neighbour11 = new Vector3(position.x + h, position.y, position.z + h);
        Vector3 neighbour12 = new Vector3(position.x, position.y + h, position.z + h);
        Vector3 neighbour13 = new Vector3(position.x, position.y - h, position.z + h);
        Vector3 neighbour14 = new Vector3(position.x - h, position.y + h, position.z + h);
        Vector3 neighbour15 = new Vector3(position.x - h, position.y - h, position.z + h);
        Vector3 neighbour16 = new Vector3(position.x - h, position.y, position.z + h);
      

        Vector3 neighbour17 = new Vector3(position.x + h, position.y + h, position.z - h);
        Vector3 neighbour18 = new Vector3(position.x + h, position.y - h, position.z - h);
        Vector3 neighbour19 = new Vector3(position.x + h, position.y, position.z - h);
        Vector3 neighbour20 = new Vector3(position.x, position.y + h, position.z - h);
        Vector3 neighbour21 = new Vector3(position.x, position.y - h, position.z - h);
        Vector3 neighbour22 = new Vector3(position.x - h, position.y + h, position.z - h);
        Vector3 neighbour23 = new Vector3(position.x - h, position.y - h, position.z - h);
        Vector3 neighbour24 = new Vector3(position.x - h, position.y, position.z - h);

        Vector3 neighbour25 = new Vector3(position.x, position.y, position.z-h);
        Vector3 neighbour26 = new Vector3(position.x, position.y, position.z+h);
        //print(position+"--"+neighbour1+ "--" + neighbour2+"--" + neighbour3+"--" + neighbour4+"--" + neighbour5+"--" + neighbour6+"--" + neighbour7+"--" + neighbour8);

        neighbours.Add(giveHashId(neighbour1));
        neighbours.Add(giveHashId(neighbour2));
        neighbours.Add(giveHashId(neighbour3));
        neighbours.Add(giveHashId(neighbour4));
        neighbours.Add(giveHashId(neighbour5));
        neighbours.Add(giveHashId(neighbour6));
        neighbours.Add(giveHashId(neighbour7));
        neighbours.Add(giveHashId(neighbour8));

        neighbours.Add(giveHashId(neighbour9));
        neighbours.Add(giveHashId(neighbour10));
        neighbours.Add(giveHashId(neighbour11));
        neighbours.Add(giveHashId(neighbour12));
        neighbours.Add(giveHashId(neighbour13));
        neighbours.Add(giveHashId(neighbour14));
        neighbours.Add(giveHashId(neighbour15));
        neighbours.Add(giveHashId(neighbour16));

        neighbours.Add(giveHashId(neighbour17));
        neighbours.Add(giveHashId(neighbour18));
        neighbours.Add(giveHashId(neighbour19));
        neighbours.Add(giveHashId(neighbour20));
        neighbours.Add(giveHashId(neighbour21));
        neighbours.Add(giveHashId(neighbour22));
        neighbours.Add(giveHashId(neighbour23));
        neighbours.Add(giveHashId(neighbour24));

        neighbours.Add(giveHashId(neighbour25));
        neighbours.Add(giveHashId(neighbour26));

        //print(neighbours[0] + "--" + neighbours[1] + "--" + neighbours[2] + "--" + neighbours[3] + "--" + neighbours[4] + "--" + neighbours[5] + "--" + neighbours[6] + "--" + neighbours[7] + "--" + neighbours[8]);





        return neighbours;

    }

    //With this method we turn the Int[] into a List<Particle>, we also detect the good and the bad neighbours
    //The bad neighbours are those who are farther than h, if they are not in our field of focus, we remove them
    //We always need to just get the list with only the particles we need
    public List<Particle> getListNeighbours(Particle particle)
    {
        List<Particle> neighbours = new List<Particle>();

        List<int> neighboursDirections = findNeighbours(particle.hashId, particle.particle.transform.position);
        
        for (int i = 0; i < neighboursDirections.Count; i++)
        {
            if (spatialHashCollisions.ContainsKey(neighboursDirections[i]))
            {
                foreach (Particle p in (List<Particle>)spatialHashCollisions[neighboursDirections[i]])
                {

                    if (p.particle != null)
                    {

                        float distance = (particle.particle.transform.position - p.particle.transform.position).magnitude;
                        
                        float hraised = 2f;
                        if (p.idPart != particle.idPart && !neighbours.Contains(p) && distance <= hraised)
                        {
                            neighbours.Add(p);
                        }
                    }

                }
            }
        }

        return neighbours;
    }


    //This method takes our position and gives the necessary int to enter in our hash's bucket
    public int giveHashId(Vector3 position)
    {
        int prime1 = 73856093;
        int prime2 = 19349663;
        int prime3 = 83492791;

        int aux1 = prime1 * (int)position.x;
        int aux2 = prime2 * (int)position.y;
        int aux3 = prime3 * (int)position.z;

        int result = aux1 ^ aux2 ^ aux3;


        int r = result % numberOfCellsInHash;
        return r < 0 ? r + numberOfCellsInHash : r;
    }
    public class Particle
    {
        public int idPart;
        public GameObject particle;
        public Vector3 velocity;
        public Vector3 gravity;
        public int hashId = 0;
        public float mass = 1f;
        public float density;
        public Vector3 aceleration;
        public List<Particle> neighbours;
        public int collisions;
        public Particle()
        {

            particle = null;
            gravity = new Vector3(0, 0, 0);
            velocity = new Vector3(0, 0, 0);
            aceleration = new Vector3(0, 0, 0);
            density = 0;
            neighbours = new List<Particle>();
            collisions = 0;
        }
    }
    // Use this for initialization
    void Start()
    {
        id1 = 1;
        vectorEmitter.x = emissionObject.transform.position.x;
        if (numberOfCollisionsPermitted == 0)
        {
            collisionsOn = false;
        }
        else
        {
            collisionsOn = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (!RigidBodySimulation)
            {
                StartCoroutine(GenerateParticlesStream(numberOfParticles));
            }
            else
            {
                StartCoroutine(GenerateParticlesStreamRGBody(numberOfParticles));
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(GenerateParticlesExplosion(numberOfParticles));

        }


    }


    IEnumerator GenerateParticlesStreamRGBody(int number)
    {
        int particlesLaunched = 0;
        while (number > particlesLaunched)
        {
            Particle part = new Particle();
            part.particle = Instantiate(particle, vectorEmitter, Quaternion.identity) as GameObject;
            Rigidbody rigidbody = part.particle.GetComponent<Rigidbody>();
            rigidbody.AddForce(new Vector3(Random.Range(-velocity * 100, velocity * 100), Random.Range(-velocity * 100, velocity * 100), Random.Range(-velocity * 100, velocity * 100)));
            StartCoroutine(Fade(part));
            particlesLaunched++;
            print(part.particle.transform.position);
            yield return null;
        }

    }

    IEnumerator GenerateParticlesStream(int number)
    {

        int launched = 0;
        while (number > launched)
        {
            
            Particle part = new Particle();
            part.particle = Instantiate(particle, emissionObject.transform.position, Quaternion.identity) as GameObject;
            part.idPart = id1;

            part.particle.GetComponent<Rigidbody>().isKinematic = true;

            StartCoroutine(MoveParticle(part, false));


            StartCoroutine(Fade(part));
            launched++;
            id1++;
            yield return null;
        }


    }

    IEnumerator GenerateParticlesExplosion(int number)
    {

        int launched = 0;
        while (number > launched)
        {

            Particle part = new Particle();
            part.particle = Instantiate(particle, emissionObject.transform.position, Quaternion.identity) as GameObject;

            part.particle.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(MoveParticle(part, true));
            part.idPart = id1;
            StartCoroutine(Fade(part));
            launched++;
            id1++;
        }
        yield return null;

    }

    IEnumerator MoveParticle(Particle particle, bool isExplosion)
    {

        particle.gravity = new Vector3(0, -gravity, 0);
        Vector3 randomVector = new Vector3(0, 0, 0);

        if (manualVelocities)
        {
            particle.velocity.x = velocityOfXAxis + Random.Range(0, velocityOfXAxis) * randomness;
            particle.velocity.y = velocityOfYAxis + Random.Range(0, velocityOfYAxis) * randomness;
            particle.velocity.z = velocityOfZAxis + Random.Range(0, velocityOfZAxis) * randomness;
        }
        else
        {

            particle.velocity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * velocity), Mathf.Sin(Mathf.Deg2Rad * velocity), Mathf.Sin(Mathf.Deg2Rad * velocity) * Random.Range(-100, 100));

        }


        float randomScale = 0.1f;
        while (particle.particle != null)
        {


            if (particle.particle.transform.position.Equals(emissionObject.transform.position))
            {


                randomVector = Random.insideUnitSphere.normalized;
                if (isExplosion == false)
                {
                    randomVector = new Vector3(randomVector.x, Mathf.Abs(randomVector.y), randomVector.z);
                }


                particle.particle.transform.position = randomVector;

                if (isRandomScale)
                {
                    randomScale = Random.Range(0.1f, 0.5f);
                }

                particle.particle.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                particle.velocity = velocity * randomVector;

                particle.hashId = giveHashId(particle.particle.transform.position);
                addToHash(particle.hashId, particle);
                yield return null;

            }
            else
            {


                particle.particle.transform.position = (0.5f * particle.gravity * Mathf.Pow(Time.deltaTime, 2)) + particle.velocity * Time.deltaTime + particle.particle.transform.position;
                particle.velocity += particle.gravity * Time.deltaTime;
                particle.hashId = giveHashId(particle.particle.transform.position);
                addToHash(particle.hashId, particle);

                if (collisionsOn)
                {
                    Vector3 particleCenter = particle.particle.transform.position;
                    Vector3 particleExtents = particle.particle.GetComponent<Renderer>().bounds.max; //particle.particle.GetComponent<SpriteRenderer>().bounds.max;

                    Vector3 radiusVector = particleExtents - particleCenter;

                    float radius = radiusVector.sqrMagnitude;


                    particle.neighbours = getListNeighbours(particle);
                    
                    if (particle.neighbours.Count != 0)
                    {
                        
                        int i = 1;
                        while (i < particle.neighbours.Count)
                        {
                            Particle part2 = particle.neighbours[i];
                            
                            float distance = 100000;
                            if (particle.particle != null && part2.particle != null)
                            {
                                distance = (particle.particle.transform.position - part2.particle.transform.position).sqrMagnitude;
                            }


                            //here we detect the collision and now we are going to give the formula of elastic collisions following momentum
                            /*
                             * 
                             * 
                             * 
                             */
                            if (radius * 2 >= distance && distance != 0)
                            {
                                if (particle.collisions == numberOfCollisionsPermitted)
                                {

                                    Destroy(particle.particle);
                                }

                                /* Now we proceed to calculate the result vectors of velocity 
                                 * first we take the position and velocity of both particles
                                 */

                                Vector3 x1 = particle.particle.transform.position;
                                Vector3 x2 = part2.particle.transform.position;
                                Vector3 v1 = particle.velocity;
                                Vector3 v2 = part2.velocity;




                                Vector3 v_aux = v1 - v2;
                                Vector3 x_aux = x1 - x2;

                                float x_aux_abs = Mathf.Abs(x_aux.x) + Mathf.Abs(x_aux.y);
                                //We calculate the new velocity v1' = dot product of (v1-v2,x1-x2)
                                //                                   ----------------------------- * (x1-x2)
                                //                                          manhattan(x1,x2)^2
                                Vector3 vNew = (Vector3.Dot(v_aux, x_aux) / Mathf.Pow(x_aux_abs, 2)) * x_aux;

                                particle.velocity = vNew * elasticity;
                                //We turn time into 0 to make the collision the start point of movement
                                particle.collisions++;

                            }
                            i++;
                        }


                    }
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }


    IEnumerator Grow(float times, GameObject obj, float waitTime)
    {

        for (float i = 1 / times; i <= 1; i += 1 / times)
        {
            if (obj != null)
            {
                obj.transform.localScale = new Vector3(i, i, i);
            }

            yield return new WaitForSeconds(waitTime / times);

        }



    }

    IEnumerator Fade(Particle obj)
    {

        float f = lifetime + Random.Range(0, lifetime * randomness);
        /*for (int i = 0; i < particleEvolution.Length; i++)
        {

            if (!isRandomScale)
            {
                StartCoroutine(Grow(10, obj.particle, f));
            }


        if (renderer != null) { 
            renderer.sprite = particleEvolution[i];
            if (i == 2)
            {

                yield return new WaitForSeconds((f * 0.6f));

            }
            else
            {
                yield return new WaitForSeconds(f * 0.40f / (particleEvolution.Length - 1));

            }
        }
    }*/


        StartCoroutine(Grow(5, obj.particle, f));

        RemoveFromHash(obj);
        Destroy(obj.particle, f);

        yield return null;


    }

    void RemoveFromHash(Particle part)
    {
        spatialHashCollisions.Remove(part);
    }
}