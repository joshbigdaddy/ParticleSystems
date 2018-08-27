using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SPHv2 : MonoBehaviour
{

    //Public values that can be modified in the inspector while running the script
    public float interaction_radius = 0.2f;
    public List<Particle> allParticlesInSimulation;
    public int num_particles_simulation = 1000;
    public GameObject source_component;
    public Vector2 top_right_constraint = new Vector2(0, 0);
    public Vector2 top_left_constraint = new Vector2(8, 0);
    public Vector2 bot_right_constraint = new Vector2(0, 8);
    public Vector2 bot_left_constraint = new Vector2(8, 8);
    public float gravity;
    public float dt = 0.003f;
    public float gas_constant = 100;
    public float viscosity_constant = 100;
    //Private values that we are going to set internally and won't be available for the user in the inspector
    private Hashtable spatialHash = new Hashtable();
    private int count = 0;
    //Public values of X, Y and Z for initialization
    private float x;
    private float y;
    private float z;


    //Our class particle, in which we store every parameter we need to use when computing our simulation
    public class Particle
    {
        public int idPart;
        public GameObject particle;
        public Vector3 velocity;
        public Vector3 gravity;
        public int hashId = 0;
        public float mass = 0.01f;
        public float density {
            get; set; }
        public Vector3 acceleration;
        public List<Particle> neighbours;
        public int collisions;
        public Vector3 evaluationVelocity;
        public int positionInHash;
        public int coef_reduction;
        public int coef_delete_reduction;
       
        //Constructor
        public Particle()
        {

            particle = null;
            gravity = new Vector3(0, 0, 0);
            velocity = new Vector3(0, 0, 0);
            evaluationVelocity = new Vector3(0, 0, 0);
            acceleration = new Vector3(0, 0, 0);
            density = 0;
            neighbours = new List<Particle>();
            collisions = 0;
            coef_reduction = 1;
            coef_delete_reduction = 0;
        }
    }

    //Here we are going to break all the space we want in small pieces, then we are going to make a projection of our particles in a 2D Grid so as we can calculate
    //our neighbors in 2D to make the operations quicker, once we got our potential neighbors we discard all of the ones that are not optimal
    public void getPositionInHash(Particle p)
    {
        int result = 0;
        Vector2 projection = new Vector2(p.particle.gameObject.transform.position.x, p.particle.gameObject.transform.position.z);
        result = (int)(Mathf.Floor(projection.x / (interaction_radius * 2)) * 10 + Mathf.Floor(projection.y / (interaction_radius * 2)));
        p.positionInHash= result;
    }

    List<Particle> findPotentialNeighbors(Particle p, Hashtable spatialHash, int positionInHash)
    {
        int[] neighbors = new int[] { -4, -3, -2, -1, 1, 2, 3, 4 };
        List<Particle> potentialNeighbors = new List<Particle>();
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (spatialHash.ContainsKey(positionInHash + neighbors[i]))
            {
                potentialNeighbors.AddRange((List<Particle>)spatialHash[positionInHash + neighbors[i]]);
            }
        }

        return potentialNeighbors;

    }
    //We are going to add to the given position our particle and set (if not already set) an empty list with our particle inside
    public void addToHash(Particle p)
    {
        if (spatialHash.ContainsKey(p.positionInHash))
        {
           List<Particle> particlesInHashPosition=(List<Particle>) spatialHash[p.positionInHash];
           particlesInHashPosition.Add(p);
           spatialHash[p.positionInHash] = particlesInHashPosition;
        }
        else
        {
            List<Particle> particlesInHashPosition = new List<Particle>();
            particlesInHashPosition.Add(p);
            spatialHash.Add(p.positionInHash, particlesInHashPosition);
        }
    }
    //Here we are going to take our constraints and generate diferent initial positions taking into account 
    public Vector3 initializePositionWithConstraints()
    {
        Vector3 result;
        //top_left(x,y) ================== top_right (x,y)
        // ||                                    ||
        // ||                                    ||      
        // ||                                    ||
        // ||                                    ||
        // ||                                    ||
        //bot_left(x,y) ================== bot_right (x,y)
        //Note: It is thought to represent a rectangle parallel to the x axis and the y axis to make everything faster and easier to calculate

        float x = Random.Range(bot_left_constraint.x, bot_right_constraint.x);
        float z = Random.Range(bot_right_constraint.y, top_right_constraint.y);
        float y = Random.Range(0, 10);
        return result = new Vector3(x, y, z);

    }
    
    public Vector3 InitializeFlow()
    {
        Vector3 result = new Vector3(x, y, z);
        x += interaction_radius;
        if (x > 9)
        {
            x = 1;
            z += interaction_radius;

            if(z > 9)
            {
                z = 1;
                y += interaction_radius;
            }
        }
        return result ;


    }
    //This method calculates if the particle can move to the position given taking into account those constraints 
    public bool calculateConstraints(Vector3 position)
    {
        bool result = true;
        float z1 = bot_left_constraint.y;
        float x1 = bot_left_constraint.x;
        float z2 = top_right_constraint.y;
        float x2 = bot_right_constraint.x;
        float y = 0f;
        if (position.y<y || position.z > z1 || position.z < z2 || position.x > x1 || position.x < x2)
        {
            result = false;
        }
        return result;
    }

    // Use this for initialization.
    //Here we are going to populate our scene with particles and we are going to initialize everything here
    void Start()
    {
        count= 0;
        x = 1;
        y = 2+(num_particles_simulation/100);
        z = 1;
        int id1 = 1;
        allParticlesInSimulation = new List<Particle>();
        for (int i = 0; i < num_particles_simulation; i++)
        {
             Vector3 initializedPositionWithConstraints = initializePositionWithConstraints();//Here we get a Vector with all the initial positions we are going to give to our particles
            //Vector3 initializedPositionWithConstraints = InitializeFlow();
            Particle particle = new Particle();
            particle.particle = Instantiate(source_component, initializedPositionWithConstraints, Quaternion.identity) as GameObject;
            allParticlesInSimulation.Add(particle);
            particle.idPart = id1;
            id1++;
        }
        //Use this method for hive-like behaviours 
        //Initialize(allParticlesInSimulation.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        // Use this method to hive-like behaviours
        //Integrate(0.1f,allParticlesInSimulation.ToArray());

        //We first set to empty our spatial hash
       spatialHash = new Hashtable();
        
        foreach (Particle p in allParticlesInSimulation)
        {
            addToHash(p);
        }
        foreach (Particle p in allParticlesInSimulation)
        {
            calculateNeighbors(p);
            calculatePressures(p);
            p.acceleration = calculateSPH(p);//sph_final_force
            LeapFrogIntegration(p);
        }
        count++;
    }


    private float _softeningLengthSquared;


    public float SofteningLength { get; set; }
    

    //With this two methods we can generate something similar to a hive of bees or flies, so its very interesting to be used for insects simulations
    public void Initialize(Particle[] parts)
    {

        SofteningLength = 1;
        _softeningLengthSquared = SofteningLength * SofteningLength;


        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].acceleration = Vector3.zero;

            for (int j = (i + 1); j < parts.Length; j++)
            {
                Vector3 r = parts[j].particle.gameObject.transform.position - parts[i].particle.gameObject.transform.position;
                float rm = r.sqrMagnitude + _softeningLengthSquared;
                Vector3 f = r / (float) (Mathf.Sqrt(rm) * rm);
                parts[i].acceleration += f * parts[j].mass;
                parts[j].acceleration -= f * parts[i].mass;
            }
        }
    }

    public void Integrate(float dT, Particle[] bodies)
    {
        float dT2 = dT / 2.0f;

        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].velocity += bodies[i].acceleration * dT2;
            bodies[i].particle.gameObject.transform.position += bodies[i].velocity * dT;
            bodies[i].acceleration = Vector3.zero;

            for (int j = (i + 1); j < bodies.Length; j++)
            {
                Vector3 r = bodies[j].particle.gameObject.transform.position - bodies[i].particle.gameObject.transform.position;
                float rm = r.sqrMagnitude + _softeningLengthSquared;
                Vector3 f = r / (float) (Mathf.Sqrt(rm) * rm);
                bodies[i].acceleration += f * bodies[j].mass;
                bodies[j].acceleration -= f * bodies[i].mass;
            }
            bodies[i].velocity += bodies[i].acceleration * dT2;
        } 
    }

    //Integration of SPH forces with Leapfrog used in the calculus of positions
    public void calculateNeighbors(Particle particle)
    {

        //BruteForce test

        List<Particle> parts = new List<Particle>();

        foreach (Particle p in allParticlesInSimulation)
        {
            if (p.idPart != particle.idPart && (p.particle.gameObject.transform.position - particle.particle.gameObject.transform.position).magnitude <= interaction_radius)
            {
                parts.Add(p);
            }
        }

        particle.neighbours = parts;
        /*List<Particle> potentialNeighbors = findPotentialNeighbors(p,spatialHash,p.positionInHash);
        List<Particle> potentialNeighbors_copy = new List<Particle>();
        foreach (Particle potentialNeighbor in potentialNeighbors)
        {
            if ((potentialNeighbor.particle.gameObject.transform.position-p.particle.gameObject.transform.position).sqrMagnitude<=interaction_radius*interaction_radius)
            {
                potentialNeighbors_copy.Add(potentialNeighbor);
            }
        }
        p.neighbours = potentialNeighbors;*/
    }

    //Calculate Pressure forces
    public void calculatePressures(Particle particle)
    {
        float alpha, mass, h, amplitude, kernel, mod_distance, ctr_distance, density_i, density_j;
        Vector3 position_i, position_j, distance;

        distance = new Vector3(0, 0, 0);
        position_i = new Vector3(0, 0, 0);
        position_j = new Vector3(0, 0, 0);
        density_i = 0.0f;

        position_i = particle.particle.transform.position;
        mass = particle.mass;
        h = interaction_radius;
        alpha = 1.08f * (h * h);
        amplitude = ((315.0f * alpha) / (float) (64.0f * (Mathf.PI * Mathf.Pow(interaction_radius, 7))));

        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            position_j = particle.neighbours[i].particle.transform.position;
            distance = position_j - position_i;
            mod_distance = distance.magnitude;
            ctr_distance = Mathf.Pow(((h * h) - (mod_distance * mod_distance)), 3);
            kernel = amplitude * ctr_distance;
            density_j = mass * kernel;
            density_i += density_j;
        }
        particle.density=density_i;
    }

    //Calculate forces for viscosity and pressure and compute them with gravity (our external force)
    public Vector3 calculateSPH(Particle p)
    {
        Vector3 result_force=  new Vector3 (0,-gravity ,0);

        Vector3 fpi = new Vector3(0, 0, 0);
        Vector3 fv = new Vector3(0, 0, 0);
        if (count > 1){ 
        fpi = calculate_fpi(p);
        //Then fv
        
        fv = calculate_fv(p);
        }
        //Finally we get the whole aceleration

       // print("Gravedad" + gravity + " FPI: " + fpi + "FV: " + fv);
        result_force += fpi - fv;

        //print("fpv" + fv + "--fpi" + fpi);

        if (float.IsNaN(fv.magnitude)|| float.IsNaN(fpi.magnitude))
        {

        }
        return result_force;
    }
    public Vector3 calculate_fpi(Particle particle)
    {
        float beta, modGradient_ij, mod_distance, density_i, density_j, K, mod_Force, density_max;
        Vector3 distance, fpj, fpi_final, Gradient_ij;

        beta = 1.768f * interaction_radius;
        K = 1000.0f;
        modGradient_ij = (15.0f * beta) / (float) (Mathf.PI * Mathf.Pow(interaction_radius, 5));

        fpj = new Vector3(0, 0, 0);
        fpi_final = new Vector3(0, 0, 0);
        distance = new Vector3(0, 0, 0);
        Gradient_ij = new Vector3(0, 0, 0);

        density_i = particle.density;
        density_max = density_i;


        for (int i = 0; i < particle.neighbours.Count; i++)
        {
            //distance es la distancia entre dos partiículas vecinas.
            density_j = particle.neighbours[i].density;

            //Cálculo densidad maxima
            if (density_j > density_max)
            {
                density_max = density_j;
            }
            

            distance = particle.particle.transform.position - particle.neighbours[i].particle.transform.position;
            mod_distance = distance.magnitude;

            //Cálculo del gradiente del kernel.
            modGradient_ij = modGradient_ij * Mathf.Pow((Mathf.Pow(interaction_radius, 2) - Mathf.Pow(mod_distance, 2)), 2);
            Gradient_ij = (1.0f / (float) mod_distance) * distance;
            Gradient_ij = modGradient_ij * Gradient_ij;

            //Cálculo de la fuerza de presion.
            mod_Force = -K * particle.mass;
            mod_Force = mod_Force * ((density_j + density_i) / (float) (2.0f * density_max));
            fpj = mod_Force * Gradient_ij;
            fpi_final += fpj;
        }

        return fpi_final;
    }
    public Vector3 calculate_fv(Particle particle)
    {
        float gamma, mu_i, modLaplacian_ij, modVelocities_ij, mod_distance, mod_Force;
        Vector3 distance, velocity, termVelocity, fvj, fvi_final;

        gamma = 31.16f;
        mu_i = 0.36f;
        modLaplacian_ij = (318.0f * gamma) / (float) ((7.0f * Mathf.PI) * Mathf.Pow(interaction_radius, 2));


        fvj = new Vector3(0, 0, 0);
        fvi_final = new Vector3(0, 0, 0);
        distance = new Vector3(0, 0, 0);
        velocity = new Vector3(0, 0, 0);
        termVelocity = new Vector3(0, 0, 0);

        for (int i = 0; i < particle.neighbours.Count; i++)
        {

            velocity = particle.neighbours[i].velocity - particle.velocity;

            distance = particle.particle.transform.position - particle.neighbours[i].particle.transform.position;
            mod_distance = distance.magnitude;

            termVelocity = (1.0f / (float) Mathf.Pow(mod_distance, 2)) * velocity;
            termVelocity = (mu_i * particle.mass) * termVelocity;

            //Cálculo del Laplaciano del kernel.
            modLaplacian_ij = modLaplacian_ij * (Mathf.Pow(interaction_radius, 2) - Mathf.Pow(mod_distance, 2));
            modLaplacian_ij = modLaplacian_ij * Vector3.Dot(termVelocity, distance);
            fvj = modLaplacian_ij * distance;
            

            //Cálculo de la fuerza de viscosidad.

            fvi_final += fvj;
        }

        return fvi_final;
    }
    //Integration of Forces with Leapfrog, this is done to perform the movement in our system
    //This is our last step in the integration
    public void LeapFrogIntegration(Particle p)
    {
        
        Vector3 nextvelocity = p.acceleration/ (float) p.mass;
        p.acceleration = nextvelocity;

        nextvelocity *= dt;
        nextvelocity += p.velocity;
        p.evaluationVelocity = p.velocity;
        p.evaluationVelocity += nextvelocity;
        p.evaluationVelocity *= 0.5f;
        p.velocity = nextvelocity;
        nextvelocity *= dt / 5f;

        //Speed Limiting
        /*if (float.IsInfinity(nextvelocity.sqrMagnitude) || float.IsNaN(nextvelocity.sqrMagnitude))
        {
            nextvelocity =new Vector3(0, Random.Range(-1,1), 0);
        }*/
        /*if (nextvelocity.x+nextvelocity.y+nextvelocity.z> 1 || nextvelocity.x + nextvelocity.y + nextvelocity.z <-1)
        {
            nextvelocity = nextvelocity * 0.01f;
            p.evaluationVelocity = nextvelocity;
            p.velocity = nextvelocity;
        }*/
        //Constraints with the recipent

        bool constraints = calculateConstraints(p.particle.gameObject.transform.position + nextvelocity);
        if (constraints)
        {
        
            p.particle.gameObject.transform.position += nextvelocity;
            
        }
        else //we detect a collision
        {

           
            //We detect the plane our particle is colliding and then we proceed to give the normal to that plane so as we can
            //give a simple correction to our particle to set it just over that plane.
            Vector3 normal = planeNormalAfterCollision(p.particle.gameObject.transform.position + nextvelocity);
            //once we got our normal vector calculated, we proceed to take the new velocity of our particle after that collision.
            //we are going to apply a non-elastic collision with a reduction coefficient obtained with testing over our iterations.
            Vector3 newPositionAfterCollision = recalculatePosition(p.particle.gameObject.transform.position + nextvelocity,normal);
            p.acceleration = new Vector3(0, 0, 0);
            p.particle.gameObject.transform.position = newPositionAfterCollision;
            p.velocity = collided(nextvelocity,normal);
        }
          

        getPositionInHash(p);
        addToHash(p);
    }

    public Vector3 planeNormalAfterCollision(Vector3 position)
    {

        Vector3 normal = new Vector3(0, 0, 0);
        if (position.y<0)
        {
            normal.y=1;
        }
        
            float z1 = bot_left_constraint.y;
            float x1 = bot_left_constraint.x;
            float z2 = top_right_constraint.y;
            float x2 = bot_right_constraint.x;


            //TODO OBTAIN NORMAL OF PLANES
        if (position.x > x1)
            {
                normal.x=1;
            }
            else if(position.x < x2)
            {
                normal.x = -1;
            }
        if (position.z > z1)
            {
                normal.z=-1;
            }
            else if (position.z <z2)
            {
                normal.z=1;
            }
        
        return normal;
    }

    public Vector3 recalculatePosition(Vector3 position, Vector3 normal)
    {
        /*Si cogemos y miramos como es la normal, lo unico que tenemos que hacer es subir n veces la normal sumandola al vector, hasta llegar al valor de x,y o z deseado*/
        /*Mas facil, si sobrepasa x numero del plano que lo define, por ejemplo si x es mayor que 8, pos cogemos y lo ponemos a 7,66 y listo*/
        float z1 = bot_left_constraint.y;
        float x1 = bot_left_constraint.x;
        float z2 = top_right_constraint.y;
        float x2 = bot_right_constraint.x;

        Vector3 result = position;
        if (normal.x==1)
        {
            result.x = x1;
        }
        else if(normal.x == -1){
            result.x = x2;
        }
        if (normal.y == 1)
        {
            result.y = 0;
        }
        if (normal.z == -1)
        {
            result.z = z1;
        }
        else if (normal.z == 1)
        {
            result.z = z2;
        }
        return result;
    }

    public Vector3 collided( Vector3 nextvelocity,Vector3 normal)
    {
        Vector3 result;
        result = new Vector3(0, 0, 0);
        Vector3 componente_normal_velocidad = Vector3.Dot(nextvelocity, normal) * normal;
        Vector3 componente_tangencial_velocidad = nextvelocity - componente_normal_velocidad;
        result = componente_tangencial_velocidad * 0.1f - componente_normal_velocidad * 0.1f;
        //result.y *=100;
        return result;
    }
}


