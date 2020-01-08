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
    NativeArray<float3> positionArray2;
    NativeArray<int> newCellIndex;

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

                positionArray.Dispose();
                directionArray.Dispose();
                
                velocityArray.Dispose();
                targetPositionArray.Dispose();
                hasTargetArray.Dispose();

                for (int i = 0; i < Count; i++)
                {
                    foodNeedsSum += boidsList[i].foodNeeds;
                    boidsList[i].UpdateBoid(accelerationArray[i]);
                }

                accelerationArray.Dispose();            
            }

            // calculate all new cell positions
            // error if raster larger then 1 - 1 - 1
            List<List<Boid>> tmpList = new List<List<Boid>>(cellGroups.allBoidCells);

            foreach (List<Boid> boidsList in tmpList)
            {
                int Count = boidsList.Count;
                positionArray2 = new NativeArray<float3>(Count, Allocator.TempJob);
                newCellIndex = new NativeArray<int>(Count, Allocator.TempJob);

                for (int i = 0; i < Count; i++)
                {
                    positionArray2[i] = boidsList[i].position;
                }
            
                CalculateCellPosition calculateCellPosition = new CalculateCellPosition
                {
                    positionArray2 = positionArray2,
                    newCellIndex = newCellIndex,
                    widthStep = cellGroups.widthStep,
                    heightStep = cellGroups.heightStep,
                    depthStep = cellGroups.depthStep,
                    resolution = cellGroups.resolution,
                };

                JobHandle jobHandle2 = calculateCellPosition.Schedule(Count, 64);
                jobHandle2.Complete();

                positionArray2.Dispose();

                for (int i = 0; i < Count; i++)
                {
                    Boid boid = boidsList[i];
                    int oldIndex = boid.cellIndex;
                    int newIndex = newCellIndex[i];

                    if (newIndex != oldIndex)
                    {
                        boid.cellIndex = newIndex;
                        cellGroups.allBoidCells[oldIndex].Remove(boid);
                        cellGroups.allBoidCells[newIndex].Add(boid);
                    }
                }

                newCellIndex.Dispose();
            }
        
        }

        ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
    }
}



[BurstCompile]
public struct CalculateCellPosition : IJobParallelFor
{

    [ReadOnly] public NativeArray<float3> positionArray2;
    public NativeArray<int> newCellIndex;
    [ReadOnly] public float widthStep;
    [ReadOnly] public float heightStep;
    [ReadOnly] public float depthStep;
    [ReadOnly] public float3 resolution;

    public void Execute(int index)
    {
        float newIndex = ((int)(positionArray2[index].x / widthStep) + (int)(positionArray2[index].z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(positionArray2[index].y / heightStep)));
        newIndex = Mathf.Clamp(newIndex, 0, positionArray2.Length - 1);

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