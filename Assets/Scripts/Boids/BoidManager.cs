using UnityEngine;

public class BoidManager : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    Spawner spawner;

    public BoidSettings settings;

    public float distance = 1;
    Boid[] boids;
    private int numBoids;

    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        spawner = FindObjectOfType<Spawner>();
    }

    void Start()
    {
        if (spawner.spawnBoids)
        {
            boids = FindObjectsOfType<Boid>();
            numBoids = boids.Length;
            foreach (Boid b in boids)
            {
                b.Initialize(settings, null);
            }
            ecoSystemManager.setFishCount(numBoids);
        }
    }

    void Update()
    {
        if (boids != null)
        {
           
            int quaterBoids = numBoids / 100;
            for(int x = 0; x < 100; x++) {

                int index1 = 0;
                int foodNeedsSum = 0;
                for (int indexA = (x * quaterBoids); indexA < quaterBoids * (x + 1); indexA ++) {

                    Boid actBoid = boids[indexA];   
                    for (int indexB = (x * quaterBoids); indexB < quaterBoids * (x + 1); indexB ++) {
                        if (actBoid != boids[indexB]) {
                            Boid boidB = boids[indexB];
                            Vector3 offset = boidB.position - actBoid.position;
                            float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                            if (sqrDst < settings.perceptionRadius * settings.perceptionRadius) {
                                actBoid.numPerceivedFlockmates += 1;
                                actBoid.avgFlockHeading += boidB.dir;
                                actBoid.centreOfFlockmates += boidB.position;

                                if (sqrDst < settings.avoidanceRadius * settings.avoidanceRadius) {
                                    actBoid.avgAvoidanceHeading -= offset / sqrDst;
                                }
                            }
                        }
                    }

                    if (actBoid.alife)
                    {
                        foodNeedsSum += actBoid.foodNeeds;

                        actBoid.UpdateBoid();
                    }
                    else
                    {
                        actBoid.setColor(Color.black, Color.black);
                        actBoid.setWobbleSpeed(0);
                        actBoid.transform.eulerAngles = new Vector3(180, 0, 0);
                    }
                }
                ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
            }
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}