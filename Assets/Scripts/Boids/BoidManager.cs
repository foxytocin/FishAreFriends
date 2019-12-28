using UnityEngine;

public class BoidManager : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    Spawner spawner;
    const int threadGroupSize = 512;

    public BoidSettings settings;
    public ComputeShader compute;
    private ComputeBuffer boidBuffer;
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
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", boids.Length);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            int foodNeedsSum = 0;
            for (int i = 0; i < numBoids; i++)
            {
                if (boids[i].alife)
                {
                    boids[i].avgFlockHeading = boidData[i].flockHeading;
                    boids[i].centreOfFlockmates = boidData[i].flockCentre;
                    boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                    boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                    foodNeedsSum += boids[i].foodNeeds;

                    boids[i].UpdateBoid();
                }
                else
                {
                    boids[i].setColor(Color.black, Color.black);
                    boids[i].setWobbleSpeed(0);
                    boids[i].transform.eulerAngles = new Vector3(0, 0, 90);
                }
            }
            ecoSystemManager.setfoodDemandFishes(foodNeedsSum);

            boidBuffer.Release();
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