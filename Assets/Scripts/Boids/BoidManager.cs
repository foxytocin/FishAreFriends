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
    float viewRadius;
    float avoidRadius;
    float alignWeight;
    float cohesionWeight;
    float seperateWeight;
    float maxSpeed;
    float maxSteerForce;
    float targetWeight;

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
                //List<Boid> boidsList = cellGroups.allBoidCells[j];

                int Count = boidsList.Count;
                positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                accelerationArray = new NativeArray<float3>(Count, Allocator.TempJob);
                velocityArray = new NativeArray<float3>(Count, Allocator.TempJob);
                targetPositionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                hasTargetArray = new NativeArray<bool>(Count, Allocator.TempJob);

                for (int i = 0; i < Count; i++)
                {
                    Boid boid = boidsList[i];

                    positionArray[i] = boid.position;
                    directionArray[i] = boid.forward;
                    velocityArray[i] = boid.velocity;

                    if (boid.velocity != null)
                        velocityArray[i] = boid.velocity;

                    hasTargetArray[i] = (bool)boid.target;
                    if ((bool)boid.target)
                        targetPositionArray[i] = boid.target.position;
                }

                CellBoidParallelJob cellBoidParallelJob = new CellBoidParallelJob
                {
                    positionArray = positionArray,
                    targetPositionArray = targetPositionArray,
                    hasTargetArray = hasTargetArray,
                    directionArray = directionArray,
                    accelerationArray = accelerationArray,
                    velocityArray = velocityArray,
                    viewRadius = viewRadius,
                    avoidRadius = avoidRadius,
                    alignWeight = alignWeight,
                    cohesionWeight = cohesionWeight,
                    seperateWeight = seperateWeight,
                    maxSpeed = maxSpeed,
                    maxSteerForce = maxSteerForce,
                    targetWeight = targetWeight
                };

                JobHandle jobHandle = cellBoidParallelJob.Schedule(Count, 64);
                jobHandle.Complete();

                for (int i = 0; i < Count; i++)
                {
                    foodNeedsSum += boidsList[i].foodNeeds;
                    boidsList[i].UpdateBoid(accelerationArray[i]);
                }

                positionArray.Dispose();
                directionArray.Dispose();
                accelerationArray.Dispose();
                velocityArray.Dispose();
                targetPositionArray.Dispose();
                hasTargetArray.Dispose();
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
    [ReadOnly] public NativeArray<float3> directionArray;
    public NativeArray<float3> accelerationArray;
    [ReadOnly] public NativeArray<float3> velocityArray;

    [ReadOnly] public float viewRadius;
    [ReadOnly] public float avoidRadius;
    [ReadOnly] public float alignWeight;
    [ReadOnly] public float cohesionWeight;
    [ReadOnly] public float seperateWeight;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float maxSteerForce;
    [ReadOnly] public float targetWeight;

    public void Execute(int index)
    {

        float viewRadiusSqr = viewRadius * viewRadius;
        float avoidRadiusSqr = avoidRadius * avoidRadius;
        int numFlockmates = 0;
        float3 flockHeading = new float3();
        float3 flockCentre = new float3();
        float3 separationHeading = new float3();
        float3 acceleration = new float3();

        for (int i = 0; i < positionArray.Length; i++)
        {
            if (index != i)
            {
                float3 offset = positionArray[i] - positionArray[index];
                float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (sqrDst < viewRadiusSqr)
                {
                    numFlockmates += 1;
                    flockHeading += directionArray[i];
                    flockCentre += positionArray[i];

                    if (sqrDst < avoidRadiusSqr)
                    {
                        separationHeading -= offset / sqrDst;
                    }
                }
            }
        }

        if (hasTargetArray[index])
        {
            float3 offsetToTarget = targetPositionArray[index] - positionArray[index];
            acceleration = Vector3.ClampMagnitude(math.normalizesafe(offsetToTarget) * maxSpeed - velocityArray[index], maxSteerForce) * targetWeight;
        }

        if (numFlockmates != 0)
        {
            flockCentre /= numFlockmates;
            float3 offsetToFlockmatesCentre = (flockCentre - positionArray[index]);

            float3 alignmentForce = Vector3.ClampMagnitude(math.normalizesafe(flockHeading) * maxSpeed - velocityArray[index], maxSteerForce) * alignWeight;
            float3 cohesionForce = Vector3.ClampMagnitude(math.normalizesafe(offsetToFlockmatesCentre) * maxSpeed - velocityArray[index], maxSteerForce) * cohesionWeight;
            float3 seperationForce = Vector3.ClampMagnitude(math.normalizesafe(separationHeading) * maxSpeed - velocityArray[index], maxSteerForce) * seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }
        accelerationArray[index] = acceleration;
    }
}