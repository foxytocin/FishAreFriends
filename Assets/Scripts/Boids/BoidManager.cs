using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    CellGroups cellGroups;
    EcoSystemManager ecoSystemManager;
    Spawner spawner;
    public BoidSettings settings;
    public ComputeShader compute;
    private ComputeBuffer boidBuffer;


    void Start()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        cellGroups = FindObjectOfType<CellGroups>();
        spawner = FindObjectOfType<Spawner>();
    }


    void Update()
    {
        int foodNeedsSum = 0;
        if (cellGroups.allBoidCells != null)
        {
            foreach (List<Boid> boidsList in cellGroups.allBoidCells)
            {
                if (boidsList.Count > 0)
                {
                    var boidData = new BoidData[boidsList.Count];

                    for (int i = 0; i < boidsList.Count; i++)
                    {
                        boidData[i].position = boidsList[i].position;
                        boidData[i].direction = boidsList[i].forward;
                    }

                    boidBuffer = new ComputeBuffer(boidsList.Count, BoidData.Size);
                    boidBuffer.SetData(boidData);

                    compute.SetBuffer(0, "boids", boidBuffer);
                    compute.SetInt("numBoids", boidsList.Count);
                    compute.SetFloat("viewRadius", settings.perceptionRadius);
                    compute.SetFloat("avoidRadius", settings.avoidanceRadius);

                    int threadGroups = Mathf.CeilToInt(boidsList.Count / (float)threadGroupSize);
                    compute.Dispatch(0, threadGroups, 1, 1);

                    boidBuffer.GetData(boidData);

                    for (int i = 0; i < boidsList.Count; i++)
                    {
                        if (boidsList[i].alife)
                        {
                            boidsList[i].avgFlockHeading = boidData[i].flockHeading;
                            boidsList[i].centreOfFlockmates = boidData[i].flockCentre;
                            boidsList[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                            boidsList[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                            foodNeedsSum += boidsList[i].foodNeeds;

                            boidsList[i].UpdateBoid();
                        }
                        else
                        {
                            boidsList[i].setColor(Color.black, Color.black);
                            boidsList[i].setWobbleSpeed(0);
                            boidsList[i].transform.eulerAngles = new Vector3(180, 0, 0);
                        }
                    }
                    boidBuffer.Release();
                }
            }
            ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
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