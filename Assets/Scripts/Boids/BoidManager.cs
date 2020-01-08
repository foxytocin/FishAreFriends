using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;


public class BoidManager : MonoBehaviour
{
    CellGroups cellGroups;
    EcoSystemManager ecoSystemManager;
    Spawner spawner;
    public BoidSettings settings;
    NativeArray<float3> positionArray;
    NativeArray<float3> directionArray;
    NativeArray<float3> accelerationArray;
    NativeArray<float3> velocityArray;
    NativeArray<float3> targetPositionArray;
    NativeArray<bool> hasTargetArray;
    NativeArray<float3> flockHeading;
    NativeArray<float3> flockCentre;
    NativeArray<float3> separationHeading;
    NativeArray<int> numFlockmates;
    float viewRadius;
    float avoidRadius;
    float alignWeight;
    float cohesionWeight;
    float seperateWeight;
    float maxSpeed;
    float maxSteerForce;
    float targetWeight;

    static int threadGroupSize = 512;
    public ComputeShader compute;
    private ComputeBuffer boidBuffer;

    Boid boid;
    BoidData data;

    void Awake()
    {
        viewRadius = settings.perceptionRadius;
        avoidRadius = settings.avoidanceRadius;
        alignWeight = settings.alignWeight;
        cohesionWeight = settings.cohesionWeight;
        seperateWeight = settings.seperateWeight;
        maxSpeed = settings.maxSpeed;
        maxSteerForce = settings.maxSteerForce;
        targetWeight = settings.targetWeight;
    }
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

                    int Count = boidsList.Count;
                    positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                    directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                    accelerationArray = new NativeArray<float3>(Count, Allocator.TempJob);
                    velocityArray = new NativeArray<float3>(Count, Allocator.TempJob);
                    targetPositionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                    hasTargetArray = new NativeArray<bool>(Count, Allocator.TempJob);
                    numFlockmates = new NativeArray<int>(Count, Allocator.TempJob);
                    flockHeading = new NativeArray<float3>(Count, Allocator.TempJob);
                    flockCentre = new NativeArray<float3>(Count, Allocator.TempJob);
                    separationHeading = new NativeArray<float3>(Count, Allocator.TempJob);

                    for (int i = 0; i < Count; i++)
                    {
                        data = boidData[i];
                        boid = boidsList[i];

                        // values to multithread on cpu
                        positionArray[i] = boid.position;
                        directionArray[i] = boid.forward; // forward?
                        velocityArray[i] = boid.velocity;

                        if (boid.velocity != null)
                            velocityArray[i] = boid.velocity;

                        hasTargetArray[i] = (bool)boid.target;
                        if ((bool)boid.target)
                            targetPositionArray[i] = boid.target.position;

                        // calculated values from gpu
                        numFlockmates[i] = data.numFlockmates;
                        flockHeading[i] = data.flockHeading;
                        flockCentre[i] = data.flockCentre;
                        separationHeading[i] = data.avoidanceHeading;
                    }

                    boidBuffer.Dispose();

                    CellBoidParallelJob cellBoidParallelJob = new CellBoidParallelJob
                    {
                        positionArray = positionArray,
                        targetPositionArray = targetPositionArray,
                        hasTargetArray = hasTargetArray,
                        accelerationArray = accelerationArray,
                        velocityArray = velocityArray,
                        alignWeight = alignWeight,
                        cohesionWeight = cohesionWeight,
                        seperateWeight = seperateWeight,
                        maxSpeed = maxSpeed,
                        maxSteerForce = maxSteerForce,
                        targetWeight = targetWeight,
                        numFlockmates = numFlockmates,
                        flockHeading = flockHeading,
                        flockCentre = flockCentre,
                        separationHeading = separationHeading
                    };

                    JobHandle jobHandle = cellBoidParallelJob.Schedule(Count, 64);
                    jobHandle.Complete();

                    for (int i = 0; i < Count; i++)
                    {
                        data = boidData[i];
                        boid = boidsList[i];

                        if (boid.alife)
                        {
                            foodNeedsSum += boid.foodNeeds;
                            boidsList[i].UpdateBoid(accelerationArray[i]);
                        }
                        else
                        {
                            boid.setColor(Color.black, Color.black);
                            boid.setWobbleSpeed(0);
                            boid.transform.eulerAngles = new Vector3(180, 0, 0);
                        }
                    }

                    positionArray.Dispose();
                    directionArray.Dispose();
                    accelerationArray.Dispose();
                    velocityArray.Dispose();
                    targetPositionArray.Dispose();
                    hasTargetArray.Dispose();
                    numFlockmates.Dispose();
                    flockHeading.Dispose();
                    flockCentre.Dispose();
                    separationHeading.Dispose();
                }
            }
        }

        ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
    }
}




[BurstCompile]
public struct CellBoidParallelJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positionArray;
    [ReadOnly] public NativeArray<float3> targetPositionArray;
    [ReadOnly] public NativeArray<bool> hasTargetArray;
    public NativeArray<float3> accelerationArray;
    [ReadOnly] public NativeArray<float3> velocityArray;
    [ReadOnly] public float alignWeight;
    [ReadOnly] public float cohesionWeight;
    [ReadOnly] public float seperateWeight;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float maxSteerForce;
    [ReadOnly] public float targetWeight;
    [ReadOnly] public NativeArray<int> numFlockmates;
    [ReadOnly] public NativeArray<float3> flockHeading;
    [ReadOnly] public NativeArray<float3> flockCentre;
    [ReadOnly] public NativeArray<float3> separationHeading;

    public void Execute(int index)
    {
        float3 acceleration = new float3(0);

        if (hasTargetArray[index])
        {
            float3 offsetToTarget = targetPositionArray[index] - positionArray[index];
            acceleration = Vector3.ClampMagnitude(math.normalizesafe(offsetToTarget) * maxSpeed - velocityArray[index], maxSteerForce) * targetWeight;
        }

        if (numFlockmates[index] != 0)
        {
            float3 flockCentre_ = flockCentre[index];
            flockCentre_ /= numFlockmates[index];
            float3 offsetToFlockmatesCentre = (flockCentre_ - positionArray[index]);

            float3 alignmentForce = Vector3.ClampMagnitude(math.normalizesafe(flockHeading[index]) * maxSpeed - velocityArray[index], maxSteerForce) * alignWeight;
            float3 cohesionForce = Vector3.ClampMagnitude(math.normalizesafe(offsetToFlockmatesCentre) * maxSpeed - velocityArray[index], maxSteerForce) * cohesionWeight;
            float3 seperationForce = Vector3.ClampMagnitude(math.normalizesafe(separationHeading[index]) * maxSpeed - velocityArray[index], maxSteerForce) * seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }
        accelerationArray[index] = acceleration;
    }
}


public struct BoidData
{
    public float3 position;
    public float3 direction;

    public float3 flockHeading;
    public float3 flockCentre;
    public float3 avoidanceHeading;
    public int numFlockmates;

    public static int Size
    {
        get
        {
            return sizeof(float) * 3 * 5 + sizeof(int);
        }
    }
}