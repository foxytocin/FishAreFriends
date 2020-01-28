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
    NativeArray<bool> hasLeaderArray;
    float viewRadius;
    float avoidRadius;
    float alignWeight;
    float cohesionWeight;
    float seperateWeight;
    float seperateWeightLeader;
    float maxSpeed;
    float maxSteerForce;
    float targetWeight;
    NativeArray<float3> positionArray2;
    NativeArray<int> newCellIndex;

    public float timeBetweenCellUpdates = 5f;
    private float timeTilCellUpdate;
    private bool boidInitializationCompleted = false;

    void Awake()
    {
        viewRadius = settings.perceptionRadius;
        avoidRadius = settings.avoidanceRadius;
        alignWeight = settings.alignWeight;
        cohesionWeight = settings.cohesionWeight;
        seperateWeight = settings.seperateWeight;
        seperateWeightLeader = settings.seperateWeightLeader;
        maxSpeed = settings.maxSpeed;
        maxSteerForce = settings.maxSteerForce;
        targetWeight = settings.targetWeight;
        timeTilCellUpdate = timeBetweenCellUpdates;

    }
    void Start()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        cellGroups = FindObjectOfType<CellGroups>();
        spawner = FindObjectOfType<Spawner>();
    }

    public void BoidInitializationCompleted()
    {
        timeTilCellUpdate = timeBetweenCellUpdates;
        boidInitializationCompleted = true;
    }

    void Update()
    {
        timeTilCellUpdate -= Time.deltaTime;

        if (cellGroups.allBoidCells != null)
        {
            int foodNeedsSum = 0;
            foreach (List<Boid> boidsList in cellGroups.allBoidCells)
            {
                int Count = boidsList.Count;
                positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                accelerationArray = new NativeArray<float3>(Count, Allocator.TempJob);
                velocityArray = new NativeArray<float3>(Count, Allocator.TempJob);
                targetPositionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                hasTargetArray = new NativeArray<bool>(Count, Allocator.TempJob);
                hasLeaderArray = new NativeArray<bool>(Count, Allocator.TempJob);

                for (int i = 0; i < Count; i++)
                {
                    Boid boid = boidsList[i];

                    positionArray[i] = boid.position;
                    directionArray[i] = boid.forward;
                    velocityArray[i] = boid.velocity;
                    hasLeaderArray[i] = boid.HasLeader();

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
                    hasLeaderArray = hasLeaderArray,
                    viewRadiusSqr = viewRadius * viewRadius,
                    avoidRadiusSqr = avoidRadius * avoidRadius,
                    alignWeight = alignWeight,
                    cohesionWeight = cohesionWeight,
                    seperateWeight = seperateWeight,
                    maxSpeed = maxSpeed,
                    maxSteerForce = maxSteerForce,
                    targetWeight = targetWeight,
                    seperateWeightLeader = seperateWeightLeader
                };

                JobHandle jobHandle1 = cellBoidParallelJob.Schedule(Count, 128);
                jobHandle1.Complete();

                positionArray.Dispose();
                directionArray.Dispose();

                velocityArray.Dispose();
                targetPositionArray.Dispose();
                hasTargetArray.Dispose();
                hasLeaderArray.Dispose();

                for (int i = 0; i < Count; i++)
                {
                    Boid boid = boidsList[i];

                    if (boid.alife)
                    {
                        foodNeedsSum += boid.foodNeeds;
                        boid.UpdateBoid(accelerationArray[i]);

                    }
                    else if (boid.status != Boid.Status.died)
                    {

                        boid.status = Boid.Status.died;
                        boid.setColor(Color.black, Color.black);
                        boid.setWobbleSpeed(0);
                        boid.transform.eulerAngles = new Vector3(180, 0, 0);
                    }
                }

                accelerationArray.Dispose();
            }
            ecoSystemManager.setfoodDemandFishes(foodNeedsSum);


            if (boidInitializationCompleted && timeTilCellUpdate < 0f)
            {
                timeTilCellUpdate = timeBetweenCellUpdates;

                List<List<Boid>> newList = new List<List<Boid>>();
                for (int i = 0; i < cellGroups.allBoidCells.Count; i++)
                {
                    newList.Add(new List<Boid>());
                }

                foreach (List<Boid> boidsList in cellGroups.allBoidCells)
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
                        maxCellIndex = cellGroups.allBoidCells.Count
                    };

                    JobHandle jobHandle2 = calculateCellPosition.Schedule(Count, 128);
                    jobHandle2.Complete();

                    positionArray2.Dispose();

                    for (int i = 0; i < Count; i++)
                    {
                        boidsList[i].cellIndex = newCellIndex[i];
                        newList[newCellIndex[i]].Add(boidsList[i]);
                    }

                    newCellIndex.Dispose();
                }

                cellGroups.allBoidCells = newList;
            }
        }
    }
}


[BurstCompile]
public struct CalculateCellPosition : IJobParallelFor
{

    public NativeArray<float3> positionArray2;
    public NativeArray<int> newCellIndex;
    [ReadOnly] public float widthStep;
    [ReadOnly] public float heightStep;
    [ReadOnly] public float depthStep;
    [ReadOnly] public float3 resolution;
    [ReadOnly] public int maxCellIndex;

    public void Execute(int index)
    {
        int newIndex = (int)((int)(positionArray2[index].x / widthStep) + (int)(positionArray2[index].z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(positionArray2[index].y / heightStep)));
        newIndex = Mathf.Clamp(newIndex, 0, maxCellIndex - 1);

        newCellIndex[index] = newIndex;
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
    [ReadOnly] public NativeArray<bool> hasLeaderArray;

    [ReadOnly] public float viewRadiusSqr;
    [ReadOnly] public float avoidRadiusSqr;
    [ReadOnly] public float alignWeight;
    [ReadOnly] public float cohesionWeight;
    [ReadOnly] public float seperateWeight;
    [ReadOnly] public float seperateWeightLeader;
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

        // if boid has leader, use other seperate weight
        float usedSeperateWeight = seperateWeight;
        if (hasLeaderArray[index])
            usedSeperateWeight = seperateWeightLeader;

          
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
            acceleration += (float3)Vector3.ClampMagnitude(math.normalizesafe(separationHeading) * maxSpeed - velocity, maxSteerForce) * usedSeperateWeight;
        }

        accelerationArray[index] = acceleration;
    }
}