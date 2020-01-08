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
                    viewRadiusSqr = viewRadius * viewRadius,
                    avoidRadiusSqr = avoidRadius * avoidRadius,
                    alignWeight = alignWeight,
                    cohesionWeight = cohesionWeight,
                    seperateWeight = seperateWeight,
                    maxSpeed = maxSpeed,
                    maxSteerForce = maxSteerForce,
                    targetWeight = targetWeight
                };

                JobHandle jobHandle1 = cellBoidParallelJob.Schedule(Count, 64);
                jobHandle1.Complete();

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

            // calculate all new cell positions
            int sumOfAllBoids = 0;
            foreach (List<Boid> boidsList in cellGroups.allBoidCells)
            {
                sumOfAllBoids += boidsList.Count;
            }
            print("Summe aller Boids: " +sumOfAllBoids);
            positionArray = new NativeArray<float3>(sumOfAllBoids, Allocator.TempJob);
            NativeArray<int> newCellIndex = new NativeArray<int>(sumOfAllBoids, Allocator.TempJob);

            int absoluteIndex = 0;
            foreach (List<Boid> boidsList in cellGroups.allBoidCells)
            {
                for (int i = 0; i < boidsList.Count; i++)
                {
                    positionArray[absoluteIndex + i] = boidsList[i].position;
                }
                absoluteIndex += boidsList.Count;
            }

            print("PositionArray aller Boids: " +positionArray.Length);
            
            CalculateCellPosition calculateCellPosition = new CalculateCellPosition
            {
                positionArray = positionArray,
                newCellIndex = newCellIndex,
                widthStep = cellGroups.widthStep,
                heightStep = cellGroups.heightStep,
                depthStep = cellGroups.depthStep,
                resolution = cellGroups.resolution,
            };

            JobHandle jobHandle2 = calculateCellPosition.Schedule(sumOfAllBoids, 64);
            jobHandle2.Complete();


            absoluteIndex = 0;
            foreach (List<Boid> boidsList in cellGroups.allBoidCells)
            {
                for (int i = 0; i < boidsList.Count; i++)
                {
                    Boid boid = boidsList[absoluteIndex + i];
                    int oldIndex = boid.cellIndex;
                    int newIndex = newCellIndex[absoluteIndex + i];

                    if (newIndex != oldIndex)
                    {
                        boid.cellIndex = newIndex;
                        cellGroups.allBoidCells[oldIndex].Remove(boid);
                        cellGroups.allBoidCells[newIndex].Add(boid);
                    }
                }
                absoluteIndex += boidsList.Count;
            }

            positionArray.Dispose();

        }

        ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
    }
}



[BurstCompile]
public struct CalculateCellPosition : IJobParallelFor
{

    [ReadOnly] public NativeArray<float3> positionArray;

    public NativeArray<int> newCellIndex;
    [ReadOnly] public float widthStep;
    [ReadOnly] public float heightStep;
    [ReadOnly] public float depthStep;
    [ReadOnly] public float3 resolution;

    public void Execute(int index)
    {
        float newIndex = ((int)(positionArray[index].x / widthStep) + (int)(positionArray[index].z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(positionArray[index].y / heightStep)));
        newIndex = Mathf.Clamp(index, 0, positionArray.Length - 1);

        newCellIndex[index] = (int)newIndex;
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

    [ReadOnly] public float viewRadiusSqr;
    [ReadOnly] public float avoidRadiusSqr;
    [ReadOnly] public float alignWeight;
    [ReadOnly] public float cohesionWeight;
    [ReadOnly] public float seperateWeight;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float maxSteerForce;
    [ReadOnly] public float targetWeight;
    float3 flockHeading;
    float3 flockCentre;
    float3 separationHeading;
    float3 acceleration;

    public void Execute(int index)
    {
        int numFlockmates = 0;
        flockHeading = new float3();
        flockCentre = new float3();
        separationHeading = new float3();
        acceleration = new float3();

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

        float3 velocity = velocityArray[index];
        if (hasTargetArray[index])
        {
            float3 offsetToTarget = targetPositionArray[index] - positionArray[index];
            acceleration = Vector3.ClampMagnitude(math.normalizesafe(offsetToTarget) * maxSpeed - velocity, maxSteerForce) * targetWeight;
        }

        if (numFlockmates != 0)
        {
            flockCentre /= numFlockmates;
            float3 offsetToFlockmatesCentre = (flockCentre - positionArray[index]);

            acceleration += (float3)Vector3.ClampMagnitude(math.normalizesafe(flockHeading) * maxSpeed - velocity, maxSteerForce) * alignWeight;
            acceleration += (float3)Vector3.ClampMagnitude(math.normalizesafe(offsetToFlockmatesCentre) * maxSpeed - velocity, maxSteerForce) * cohesionWeight;
            acceleration += (float3)Vector3.ClampMagnitude(math.normalizesafe(separationHeading) * maxSpeed - velocity, maxSteerForce) * seperateWeight;
        }

        accelerationArray[index] = acceleration;
    }
}