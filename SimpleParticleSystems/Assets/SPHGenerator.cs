using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




[System.Serializable]
public class SPHGenerator : MonoBehaviour
{

    [Range(1, 500)]
    public int numberOfParticles = 100;
    [Range(0, 100)]
    public float gas_constant = 1;
    public float viscosity_constant = 1;
    public float gravity = 1;
    public GameObject particle;
    [Range(100, 2000)]
    private int numberOfCellsInHash = 1000;
    private float h = 0.4100403f;
    private int id1 = 1;
    private List<Particle> particles;
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

        //print(position+"--"+neighbour1+ "--" + neighbour2+"--" + neighbour3+"--" + neighbour4+"--" + neighbour5+"--" + neighbour6+"--" + neighbour7+"--" + neighbour8);

        neighbours.Add(giveHashId(neighbour1));
        neighbours.Add(giveHashId(neighbour2));
        neighbours.Add(giveHashId(neighbour3));
        neighbours.Add(giveHashId(neighbour4));
        neighbours.Add(giveHashId(neighbour5));
        neighbours.Add(giveHashId(neighbour6));
        neighbours.Add(giveHashId(neighbour7));
        neighbours.Add(giveHashId(neighbour8));

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

                    float distance = (particle.particle.transform.position - p.particle.transform.position).magnitude;
                    float hraised = h;
                    if (p.idPart != particle.idPart && !neighbours.Contains(p) && distance <= hraised)
                    {
                        neighbours.Add(p);
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

        public Particle()
        {

            particle = null;
            gravity = new Vector3(0, 0, 0);
            velocity = new Vector3(0, 0, 0);
            aceleration = new Vector3(0, 0, 0);
            density = 0;
            neighbours = new List<Particle>();

        }
    }

    void Start()
    {

        spatialHashCollisions = new Hashtable();
        particles = new List<Particle>();
        id1 = 1;
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GenerateParticlesStream(numberOfParticles));
        }
    }


    IEnumerator GenerateParticlesStream(int number)
    {


        int launched = 0;
        float aux = 0;
        float y = 0;
        while (number > launched)
        {

            Particle part = new Particle();
            if (-2 + aux >= 0)
            {
                aux = 0;
                y += h * 0.8f;
            }
            part.particle = Instantiate(particle, new Vector3(-5 + aux, 20f - y, 0), Quaternion.identity) as GameObject;
            part.particle.GetComponent<Rigidbody2D>().isKinematic = true;

            particles.Add(part);

            part.density = 15 / Mathf.PI * h * h * h;

            part.particle.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);

            part.hashId = giveHashId(part.particle.transform.position);
            part.particle.GetComponent<Rigidbody2D>().isKinematic = true;


            aux += h * 0.8f;
            launched++;
            part.idPart = id1;
            id1++;
            StartCoroutine(MoveParticle(part));
            
        }

        yield return null;
    }
    IEnumerator MoveParticle(Particle particle)
    {


        particle.gravity = new Vector3(0, -gravity, 0);
        particle.aceleration = particle.gravity;
        while (particle.particle != null)
        {
            //First we need to calculate pi and then fpi in our simulation
            // pi is the density in our particle being calculated as the following

            // pi = sum from j to n ( mass of j* Laplacian(distance between i and j, fixed value))
            // and with the density pi, we calculate the pressure force fpi 

            particle.gravity = new Vector3(0, -gravity, 0);
            if (particle.particle.transform.position.y < -3)
            {
                particle.aceleration = new Vector3(particle.aceleration.x, 0, particle.aceleration.z);
                particle.velocity = new Vector3(particle.velocity.x, -particle.velocity.y*0.1f, particle.velocity.z);

                particle.particle.transform.position = new Vector3(particle.particle.transform.position.x, -3f, 0);
            }

            if (particle.particle.transform.position.x < -5 || particle.particle.transform.position.x > -2)
            {
                particle.aceleration = new Vector3(0, particle.aceleration.y, particle.aceleration.z);
                particle.velocity = new Vector3(-particle.velocity.x * 0.1f, particle.velocity.y, particle.velocity.z);

                if (particle.particle.transform.position.x < -5)
                {
                    particle.particle.transform.position = new Vector3(-5f, particle.particle.transform.position.y, 0);
                }
                else
                {
                    particle.particle.transform.position = new Vector3(-2f, particle.particle.transform.position.y, 0);
                }

            }


            particle.particle.transform.position += (0.5f * particle.aceleration * Mathf.Pow(0.01f, 2)) + particle.velocity * 0.01f;

            


            //Once position is recalculated we proceed to calculate the velocity of our next iteration
            particle.velocity += particle.aceleration * 0.01f;
            particle.hashId = giveHashId(particle.particle.transform.position);
            addToHash(particle.hashId, particle);

            StartCoroutine(WaveColor(particle));

            particle.aceleration = particle.gravity;


            particle.neighbours = getListNeighboursBruteForce(particle);
           // particle.neighbours = getListNeighbours(particle);

            //Now all of our densities are calculated we proceed to calculate the forces
            calculateDensity(particle);

            //Then we got 3 different forces we need to calculate,  fex + fv + fp being gravity,viscosity and pressure respectively

            //First fpi
            Vector3 fpi = new Vector3(0, 0, 0);
            //fpi = calculate_fpi(particle);
            fpi = calculate_fpiLooking4Stability(particle);
            
            //Then fv
            Vector3 fv = new Vector3(0, 0, 0);
            fv = calculate_fv(particle);
            //Finally we get the whole aceleration

            Vector3 acceleration_pressure = fpi / particle.mass;
            Vector3 acceleration_viscosity = fv / particle.mass;

            particle.aceleration = particle.gravity + 4 * acceleration_pressure + 2 * acceleration_viscosity;
            yield return null;
        }


    }
    public List<Particle> getListNeighboursBruteForce(Particle particle)
    {
        List<Particle> parts = new List<Particle>();

        foreach(Particle p in particles)
        {
            if(p.idPart!=particle.idPart && (p.particle.transform.position - particle.particle.transform.position).magnitude <= h)
            {
                parts.Add(p);
            }
        }

        return parts;
    }
        public void calculateDensity(Particle particle)
    {
        float smooth_norm = 15 / (Mathf.PI * h * h * h);
        float sumPi = smooth_norm;
        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            float distance = (particle.particle.transform.position - particle.neighbours[i].particle.transform.position).magnitude;
            float spiky_smoothing_kernel = smooth_norm * Mathf.Pow((1 - distance) / h, 3);
            sumPi += particle.neighbours[i].mass * spiky_smoothing_kernel;
        }

        particle.density = sumPi;
    }

    public Vector3 calculate_fpi(Particle particle)
    {
        Vector3 fpi_final = new Vector3(0, 0, 0);
        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            Vector3 distance = particle.particle.transform.position - particle.neighbours[i].particle.transform.position;
            float pj = gas_constant * particle.neighbours[i].density;
            Vector3 kernel = (45 / (Mathf.PI * h * h * h * h)) * Mathf.Pow(1 - distance.magnitude / h, 2) * (distance / distance.magnitude);
            fpi_final += (particle.neighbours[i].mass / pj) * ((particle.density + pj) / 2) * kernel;
        }

        return -fpi_final * (particle.mass / particle.density);
    }

    public Vector3 calculate_fpiLooking4Stability(Particle particle)
    {   //Constant that we must set between 1000 and 3600 being 10 the minimun possible and something to take in care just if necessary 
        int K = 5;
        Vector3 fpi_final = new Vector3(0, 0, 0);
        for (int i = 0; i < particle.neighbours.Count; i++)
        {
           Vector3 distance = particle.neighbours[i].particle.transform.position - particle.particle.transform.position;
           fpi_final += K * ((h - distance.magnitude) / distance.magnitude) * distance;
        }

        return fpi_final;
    }

    public Vector3 calculate_fv(Particle particle)
    {
        Vector3 fv_final = new Vector3(0, 0, 0);
        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            Vector3 distance = particle.particle.transform.position - particle.neighbours[i].particle.transform.position;


            Vector3 velocity = particle.neighbours[i].velocity - particle.velocity;
            float pj = gas_constant * particle.neighbours[i].density;

            float kernel = (45 / (Mathf.PI * Mathf.Pow(h, 6))) * (h - distance.magnitude);
            fv_final += (particle.neighbours[i].mass / particle.neighbours[i].density /*pj*/) * velocity * kernel;
        }
        return fv_final * viscosity_constant * (particle.mass / particle.density);
    }

    IEnumerator WaveColor(Particle particle)
    {
        Color white = Color.white;
        Color blue = Color.cyan;
        
        particle.particle.GetComponent< SpriteRenderer>().color=  Color.Lerp(blue, white, particle.velocity.magnitude-1f);
        
        
        yield return null;
    }
}


