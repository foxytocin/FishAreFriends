using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    public float distance = 1;
    Boid[] boids;

    void Start()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids)
        {
            b.Initialize(settings, null);
        }
        ecoSystemManager.setFishCount(boids.Length);
    }

    void Update()
    {
        if (boids != null)
        {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
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
                    // Hier wird aktuell nur das Mesh des Fishes
                    GameObject tb = boids[i].gameObject;
                    tb.SetActive(false);
                }
            }
            ecoSystemManager.setFoodDemand(foodNeedsSum);

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