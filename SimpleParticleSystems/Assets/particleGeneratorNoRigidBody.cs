using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[System.Serializable]
[CustomEditor(typeof(particleGeneratorNoRigidBody))]
public class MyScriptEditor : Editor
{
    override public void OnInspectorGUI()
    {
        var myScript = target as particleGeneratorNoRigidBody;

       

        
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
        }
        else
        {
            myScript.velocity = EditorGUILayout.FloatField("Velocity:", myScript.velocity);
        }
    }

}
[System.Serializable]
public class particleGeneratorNoRigidBody : MonoBehaviour {

    [Range(2,10)]
    public float lifetime = 2;
    [Range(0f, 1f)]
    public float waitTime = 0.3f;
    [Range(0f,1f)]
    public float randomness = 0.5f;
    [Range(1, 1000)]
    public int numberOfParticles = 100;

    [Range(10, 180)]
    public int angleRange = 90;

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
    
    private Vector3 vectorEmitter = new Vector3(0, 0, 0);

    [HideInInspector]
    public bool RigidBodySimulation = false;
    [HideInInspector]
    public float gravity = 1;
    [Range (1,20)]
    public int numberOfCollisionsPermitted = 4;

    
    public GameObject emissionObject;
    public GameObject particle;
    public bool isRandomScale;
    [Range(100,2000)]
    public int numberOfCellsInHash = 1000;
    [Range(1,100)]
    public int numberOfRows =20;
    
    
    public Sprite[] particleEvolution;


    //Privae Classes used in this method

    /**
     * This is a List with all the particles that is beeing updated whenever a particle is created or destroyed
     * **/
    private List<Particle> particles= new List<Particle>();
    //All the pixels we have in our scene


    public int getRowsSpatialHash()
    {
        return numberOfRows + (numberOfRows % numberOfCellsInHash);
    }

    public int getColumnsSpatialHash()
    {
        return numberOfCellsInHash/ getRowsSpatialHash();
    }

    /**
     * This spatialHashCollisions is going to be the dictionary in which we are going to study the collisions
     * */
    private Hashtable spatialHashCollisions = new Hashtable();

    //This method will add to our HashTable a value

    public void addToHash(Hashtable hash, int position, Particle particle)
    {
        foreach(List<Particle> l in hash.Values)
        {
            if (l.Contains(particle))
            {
                l.Remove(particle);
            }
        }
        if (!hash.ContainsKey(position))
        {
            List<Particle> partsNew = new List<Particle>();
            partsNew.Add(particle);
            hash.Add(position, partsNew);
        }
        else
        {
            List<Particle> parts= (List<Particle>)hash[position];
            parts.Add(particle);
            hash.Remove(position);
            hash.Add(position, parts);
        }

    }
   

    //Turns the (x,y) coordinates into an int that hashcollisions can read
    public int obtainIntValueFromMatrix(Vector2 coordinates,int numRows)
    {
        int value = ((int)coordinates.x * ( getRowsSpatialHash()- 1))+(int) coordinates.y;
        return value;
    }
    public int[] findNeighbours(int i)
    {
        int[] neighbours = {i,0,0,0,0,0,0,0,0};

        
        int numCols = getColumnsSpatialHash();
        int numRows = getRowsSpatialHash();

        //We get into coordinates the number we got
        Vector2 positionInHash= new Vector2(i%numRows, i%numCols);

        if (positionInHash.x==1)
        {
            if (positionInHash.y == 1)
            {
                //if the position is (1,1) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(1+1,1);
                Vector2 neighbour2 = new Vector2(1,1+1);
                Vector2 neighbour3 = new Vector2(1+1,1+1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
            }
            else if (positionInHash.y == numCols)
            {
                //if the position is (1, final) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(1 + 1, numCols);
                Vector2 neighbour2 = new Vector2(1, numCols-1);
                Vector2 neighbour3 = new Vector2(1 + 1,numCols-1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
            }
            else
            {
                //if the position is (1, whatever) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(1, positionInHash.y+1);
                Vector2 neighbour2 = new Vector2(1, positionInHash.y-1);
                Vector2 neighbour3 = new Vector2(2, positionInHash.y);
                Vector2 neighbour4 = new Vector2(2, positionInHash.y-1);
                Vector2 neighbour5 = new Vector2(2, positionInHash.y+1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
                neighbours[4] = obtainIntValueFromMatrix(neighbour4, numRows);
                neighbours[5] = obtainIntValueFromMatrix(neighbour5, numRows);
            }
        }
        else if (positionInHash.x == numRows)
        {
            if (positionInHash.y == 1)
            {
                //if the position is (final,1) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(positionInHash.x-1,1);
                Vector2 neighbour2 = new Vector2(positionInHash.x,1+1);
                Vector2 neighbour3 = new Vector2(positionInHash.x-1,1+1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
            }
            else if (positionInHash.y == numCols)
            {
                //if the position is (final,final) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(positionInHash.x-1, positionInHash.y-1);
                Vector2 neighbour2 = new Vector2(positionInHash.x, positionInHash.y-1);
                Vector2 neighbour3 = new Vector2(positionInHash.x-1, positionInHash.y);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);

            }
            else
            {
                //if the position is (final,whatever) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(positionInHash.x, positionInHash.y-1);
                Vector2 neighbour2 = new Vector2(positionInHash.x-1, positionInHash.y);
                Vector2 neighbour3 = new Vector2(positionInHash.x-1, positionInHash.y-1);
                Vector2 neighbour4 = new Vector2(positionInHash.x-1, positionInHash.y+1);
                Vector2 neighbour5 = new Vector2(positionInHash.x, positionInHash.y+1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
                neighbours[4] = obtainIntValueFromMatrix(neighbour4, numRows);
                neighbours[5] = obtainIntValueFromMatrix(neighbour5, numRows);
            }
        }
        else
        {
            if (positionInHash.y == 1)
            {
                //if the position is (whatever,1) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(positionInHash.x, positionInHash.y+1);
                Vector2 neighbour2 = new Vector2(positionInHash.x+1, positionInHash.y+1);
                Vector2 neighbour3 = new Vector2(positionInHash.x+1, positionInHash.y);
                Vector2 neighbour4 = new Vector2(positionInHash.x-1, positionInHash.y);
                Vector2 neighbour5 = new Vector2(positionInHash.x-1, positionInHash.y+1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
                neighbours[4] = obtainIntValueFromMatrix(neighbour4, numRows);
                neighbours[5] = obtainIntValueFromMatrix(neighbour5, numRows);
            }
            else if (positionInHash.y == numCols)
            {
                //if the position is (whatever, final) we calculate the neighbours to it
                Vector2 neighbour1 = new Vector2(positionInHash.x-1, positionInHash.y);
                Vector2 neighbour2 = new Vector2(positionInHash.x-1, positionInHash.y-1);
                Vector2 neighbour3 = new Vector2(positionInHash.x+1, positionInHash.y);
                Vector2 neighbour4 = new Vector2(positionInHash.x+1, positionInHash.y-1);
                Vector2 neighbour5 = new Vector2(positionInHash.x, positionInHash.y-1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
                neighbours[4] = obtainIntValueFromMatrix(neighbour4, numRows);
                neighbours[5] = obtainIntValueFromMatrix(neighbour5, numRows);
            }
            else
            {
                //if the position is (whatever,whatever) we calculate the neighbours to it 
                Vector2 neighbour1 = new Vector2(positionInHash.x+1, positionInHash.y);
                Vector2 neighbour2 = new Vector2(positionInHash.x+1, positionInHash.y+1);
                Vector2 neighbour3 = new Vector2(positionInHash.x+1, positionInHash.y-1);
                Vector2 neighbour4 = new Vector2(positionInHash.x, positionInHash.y+1);
                Vector2 neighbour5 = new Vector2(positionInHash.x, positionInHash.y-1);
                Vector2 neighbour6 = new Vector2(positionInHash.x-1, positionInHash.y+1);
                Vector2 neighbour7 = new Vector2(positionInHash.x-1, positionInHash.y);
                Vector2 neighbour8 = new Vector2(positionInHash.x-1, positionInHash.y-1);

                neighbours[1] = obtainIntValueFromMatrix(neighbour1, numRows);
                neighbours[2] = obtainIntValueFromMatrix(neighbour2, numRows);
                neighbours[3] = obtainIntValueFromMatrix(neighbour3, numRows);
                neighbours[4] = obtainIntValueFromMatrix(neighbour4, numRows);
                neighbours[5] = obtainIntValueFromMatrix(neighbour5, numRows);
                neighbours[6] = obtainIntValueFromMatrix(neighbour6, numRows);
                neighbours[7] = obtainIntValueFromMatrix(neighbour7, numRows);
                neighbours[8] = obtainIntValueFromMatrix(neighbour8, numRows);
            }
        }

        return neighbours;
    }

    //This method takes our position and gives the necessary int to enter in our hash's bucket
    public int giveIntPositionInHash(Vector3 position)
    {
        int prime1 = 73856093;
        int prime2 = 19349663;
        int prime3 = 83492791;

        int aux1 = prime1 * (int) position.x;
        int aux2 = prime2 * (int) position.y;
        int aux3 = prime3 * (int) position.z;

        int result = aux1 ^ aux2 ^ aux3;

        return result%numberOfCellsInHash;
    }
    public class Particle
    {
        
        public GameObject particle;
        public Vector3 velocity;
        public Vector3 gravity;
        public int collisions = 0;
        public int hashId = 0;

        public Particle()
        {
           particle = null;
           gravity=new Vector3(0, 0, 0);
           velocity = new Vector3(0, 0, 0);
           
        }
    }
    // Use this for initialization
    void Start()
    {

        vectorEmitter.x = emissionObject.transform.position.x;
       
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

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(GenerateParticlesRandomMovement(numberOfParticles));

        }

    }


    IEnumerator GenerateParticlesStreamRGBody(int number)
    {
        int particlesLaunched = 0;
        while (number > particlesLaunched)
        {

            Particle part = new Particle();
            part.particle = Instantiate(particle, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            Rigidbody2D rigidbody = part.particle.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(new Vector2(Random.Range(-velocity*100, velocity*100), Random.Range(0, velocity*100)));
            StartCoroutine(Fade(part.particle.GetComponent<SpriteRenderer>(), part));
            particlesLaunched++;
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
            particles.Add(part);

            part.particle.GetComponent<Rigidbody2D>().isKinematic=true;

            StartCoroutine(MoveParticle(part));

            StartCoroutine(Fade(part.particle.GetComponent<SpriteRenderer>(), part));
            launched++;
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
            particles.Add(part);
            
            StartCoroutine(MoveParticle(part));

            StartCoroutine(Fade(part.particle.GetComponent<SpriteRenderer>(), part));
            launched++;
        }
        yield return null;

    }

    IEnumerator MoveParticle(Particle particle)
    {

        particle.gravity = new Vector3(0,-gravity,0);
        Vector3 randomVector= new Vector3(0,0,0);
        float angle = 0;        

        if (manualVelocities)
        {
            particle.velocity.x = velocityOfXAxis + Random.Range(0, velocityOfXAxis) * randomness;
            particle.velocity.y = velocityOfYAxis + Random.Range(0, velocityOfYAxis) * randomness;
        }
        else
        {
           
           particle.velocity=new Vector3(Mathf.Cos(Mathf.Deg2Rad * velocity), Mathf.Sin(Mathf.Deg2Rad * velocity), 0);
          
        }
        

        float randomScale = 0.1f;
        while (particle.particle!=null)
        {
            // Particle first instantiation
            //We create all the values at first instance
                
                if (particle.particle.transform.position.Equals(emissionObject.transform.position))
                {
                    float angleRangeAux = angleRange / 2;
                    float randomAngle = Random.Range(90 - angleRangeAux, 90 + angleRangeAux);
                    randomVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle), 0);
                    particle.particle.transform.position = new Vector3(randomVector.x, Mathf.Abs(randomVector.y), 0);
                    if (isRandomScale)
                    {
                        randomScale = Random.Range(0.1f, 1);
                    }

                    particle.particle.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                    particle.velocity = velocity * randomVector;
                    angle = Vector3.Angle(new Vector3(0, 0, 0), randomVector);

                particle.hashId = giveIntPositionInHash(particle.particle.transform.position);
                addToHash(spatialHashCollisions, particle.hashId, particle);
                    yield return null;

                }
                else
                {
                
                
                particle.particle.transform.position = (0.5f * particle.gravity * Mathf.Pow(Time.deltaTime, 2)) + particle.velocity * Time.deltaTime+ particle.particle.transform.position;
                particle.velocity += particle.gravity * Time.deltaTime;
                particle.hashId = giveIntPositionInHash(particle.particle.transform.position);
                addToHash(spatialHashCollisions, particle.hashId, particle);

                    Vector3 particleCenter = particle.particle.GetComponent<SpriteRenderer>().bounds.center;
                    Vector3 particleExtents = particle.particle.GetComponent<SpriteRenderer>().bounds.max;

                    Vector3 radiusVector = particleExtents - particleCenter;

                    float radius = radiusVector.sqrMagnitude;

                    List<Particle> neighborsParts = new List<Particle>();
                    int[] neighbors = findNeighbours(particle.hashId);
                    for(int f = 0; f < neighbors.Length; f++)
                    {
                    if (spatialHashCollisions.ContainsKey(f))
                    {
                        neighborsParts.AddRange((List<Particle>)spatialHashCollisions[f]);
                    }
                    }
                if (neighborsParts.Count != 0)
                {
                    int i = 1;
                    while (i < neighborsParts.Count)
                    {
                        Particle part2 = neighborsParts[i];

                        float distance = 100000;
                        if (particle.particle!=null&& part2.particle!=null) {
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
                                particles.Remove(particle);
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
                yield return new WaitForSeconds(Time.deltaTime);
                }
        }
    }


    IEnumerator Grow(float times, GameObject obj, float waitTime)
    {
        
        for(float i = 1 / times; i <= 1; i+=1/times)
        {
            if (obj != null)
            {
                obj.transform.localScale = new Vector3(i, i, i);
            }
            
            yield return new WaitForSeconds(waitTime/times) ;
            
        }
        
        

    }

    IEnumerator Fade(SpriteRenderer renderer, Particle obj)
    {
        
            float f = lifetime + Random.Range(0, lifetime * randomness);
            for (int i = 0; i < particleEvolution.Length; i++)
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
        }
        
          particles.Remove(obj);
          Destroy(obj.particle);
        
    }


    IEnumerator GenerateParticlesRandomMovement(int number)
    {

        int launched = 0;
        while (number > launched)
        {
            Particle p = new Particle();
            p.particle = Instantiate(particle, emissionObject.transform.position, Quaternion.identity) as GameObject;
            StartCoroutine(MoveParticleRandomMovement(particle));

            if (p.particle!=null)
            {
                StartCoroutine(Fade(p.particle.GetComponent<SpriteRenderer>(), p));
            }
            launched++;
            yield return null;
        }

    }

    IEnumerator MoveParticleRandomMovement(GameObject particle)
    {
        float gravityvelocity = -gravity;
        Vector3 randomVector;
        Vector3 initialPosition = new Vector3(0, 0, 0);
        float angle = 0;
        float time = 0;
        while (particle != null)
        {

            if (particle.transform.position.Equals(emissionObject.transform.position))
            {
                randomVector = new Vector3(Random.Range(-10, 10), Random.Range(0, 100), 0);
                randomVector.Normalize();
                particle.transform.position = new Vector3(randomVector.x, Mathf.Abs(randomVector.y), 0);
                initialPosition = particle.transform.position;
                angle = Vector3.Angle(new Vector3(0, 0, 0), randomVector);
                time = Time.deltaTime;
                yield return null;
            }
            else
            {
                time += Time.deltaTime;
                time += Time.deltaTime;
                Vector3 direction = particle.transform.position;
                direction.Normalize();
                float newx = initialPosition.x;
                if (direction.x < 0)
                {
                    newx -= ((velocityOfXAxis * time)+(Random.Range(-velocityOfXAxis * time, velocityOfXAxis * time) *randomness));
                }
                else
                {
                    newx += ((velocityOfXAxis * time) + (Random.Range(velocityOfXAxis * time, velocityOfXAxis * time) * randomness));
                }



                float positionAceleration = ((0.5f * gravityvelocity * Mathf.Pow(time, 2)) + velocityOfYAxis * time + initialPosition.y)+Random.Range(-velocityOfYAxis,velocityOfYAxis)*randomness;

                particle.transform.position = new Vector3(newx, positionAceleration, 0);

                yield return null;
            }
        }
    }
}
