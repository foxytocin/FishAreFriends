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
    NativeArray<int> numFlockmatesArray;
    NativeArray<float3> flockHeadingArray;
    NativeArray<float3> flockCentreArray;
    NativeArray<float3> avoidanceHeadingArray;
    float viewRadius;
    float avoidRadius;


    void Start()
    {
        viewRadius = settings.perceptionRadius;
        avoidRadius = settings.avoidanceRadius;
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        cellGroups = FindObjectOfType<CellGroups>();
        spawner = FindObjectOfType<Spawner>();
    }

    void Update()
    {
        int foodNeedsSum = 0;
        if (cellGroups.allBoidCells != null)
        {
            for (int j = 0; j < cellGroups.allBoidCells.Count; j++)
            {
                List<Boid> boidsList = cellGroups.allBoidCells[j];

                int Count = boidsList.Count;
                positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
                numFlockmatesArray = new NativeArray<int>(Count, Allocator.TempJob);
                flockHeadingArray = new NativeArray<float3>(Count, Allocator.TempJob);
                flockCentreArray = new NativeArray<float3>(Count, Allocator.TempJob);
                avoidanceHeadingArray = new NativeArray<float3>(Count, Allocator.TempJob);

                for (int i = 0; i < Count; i++)
                {
                    positionArray[i] = boidsList[i].position;
                    directionArray[i] = boidsList[i].dir;
                }

                CellBoidParallelJob cellBoidParallelJob = new CellBoidParallelJob
                {
                    positionArray = positionArray,
                    directionArray = directionArray,
                    numFlockmatesArray = numFlockmatesArray,
                    flockHeadingArray = flockHeadingArray,
                    flockCentreArray = flockCentreArray,
                    avoidanceHeadingArray = avoidanceHeadingArray,
                    viewRadius = viewRadius,
                    avoidRadius = avoidRadius
                };

                JobHandle jobHandle = cellBoidParallelJob.Schedule(Count, 100);
                jobHandle.Complete();

                for (int i = 0; i < Count; i++)
                {
                    if (boidsList[i].alife)
                    {
                        boidsList[i].avgFlockHeading = flockHeadingArray[i];
                        boidsList[i].centreOfFlockmates = flockCentreArray[i];
                        boidsList[i].avgAvoidanceHeading = avoidanceHeadingArray[i];
                        boidsList[i].numPerceivedFlockmates = numFlockmatesArray[i];

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

                positionArray.Dispose();
                directionArray.Dispose();
                numFlockmatesArray.Dispose();
                flockHeadingArray.Dispose();
                flockCentreArray.Dispose();
                avoidanceHeadingArray.Dispose();
            }
    
            ecoSystemManager.setfoodDemandFishes(foodNeedsSum);

            // for (int j = 0; j < cellGroups.allBoidCells.Count; j++)
            // {
            //     List<Boid> boidsList = cellGroups.allBoidCells[j];

            //     int Count = boidsList.Count;
            //     for (int i = 0; i < Count; i++)
            //     {
            //         cellGroups.CheckCell(boidsList[i]);
            //     }
            // }
        }
    }
}


[BurstCompile]
public struct CellBoidParallelJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positionArray;
    [ReadOnly] public NativeArray<float3> directionArray;
    public NativeArray<int> numFlockmatesArray;
    public NativeArray<float3> flockHeadingArray;
    public NativeArray<float3> flockCentreArray;
    public NativeArray<float3> avoidanceHeadingArray;

    public float viewRadius;
    public float avoidRadius;

    public void Execute(int index)
    {

        float viewRadiusSqr = viewRadius * viewRadius;
        float avoidRadiusSqr = avoidRadius * avoidRadius;
        int numFlockmates = 0;
        float3 flockHeading = new float3();
        float3 flockCentre = new float3();
        float3 separationHeading = new float3();

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

        numFlockmatesArray[index] = numFlockmates;
        flockHeadingArray[index] = flockHeading;
        flockCentreArray[index] = flockCentre;
        avoidanceHeadingArray[index] = separationHeading;
    }
}



// using UnityEngine;
// using Unity.Collections;
// using System.Collections.Generic;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Burst;

// public class BoidManager : MonoBehaviour
// {
//     CellGroups cellGroups;
//     EcoSystemManager ecoSystemManager;
//     Spawner spawner;
//     public BoidSettings settings;
//     NativeArray<float3> positionArray;
//     NativeArray<float3> directionArray;
//     NativeArray<float3> accelerationArray;
//     NativeArray<float3> velocityArray;
//     //NativeArray<float3> targetPositionArray;
//     //NativeArray<bool> hasTargetArray;


//     void Start()
//     {
//         ecoSystemManager = FindObjectOfType<EcoSystemManager>();
//         cellGroups = FindObjectOfType<CellGroups>();
//         spawner = FindObjectOfType<Spawner>();
//     }

//     void Update()
//     {
//         int foodNeedsSum = 0;
//         if (cellGroups.allBoidCells != null)
//         {
//             foreach (List<Boid> boidsList in cellGroups.allBoidCells)
//             {
//                 //List<Boid> boidsList = cellGroups.allBoidCells[j];

//                 int Count = boidsList.Count;
//                 positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
//                 directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
//                 accelerationArray = new NativeArray<float3>(Count, Allocator.TempJob);
//                 velocityArray = new NativeArray<float3>(Count, Allocator.TempJob);
//                 //targetPositionArray = new NativeArray<float3>(Count, Allocator.TempJob);
//                 //hasTargetArray = new NativeArray<bool>(Count, Allocator.TempJob);
//                 float viewRadius = settings.perceptionRadius;
//                 float avoidRadius = settings.avoidanceRadius;
//                 float alignWeight = settings.alignWeight;
//                 float cohesionWeight = settings.cohesionWeight;
//                 float seperateWeight = settings.seperateWeight;
//                 float maxSpeed = settings.maxSpeed;
//                 float maxSteerForce = settings.maxSteerForce;
//                 float targetWeight = settings.targetWeight;

//                 for (int i = 0; i < Count; i++)
//                 {
//                     positionArray[i] = boidsList[i].position;
//                     directionArray[i] = boidsList[i].dir;
//                     velocityArray[i] = boidsList[i].velocity;

//                     // if (boidsList[i].velocity != null)
//                     // {
//                     //     velocityArray[i] = boidsList[i].velocity;
//                     // }
//                     // else
//                     // {
//                     //     velocityArray[i] = new float3(0, 0, 0);
//                     // }

//                     // hasTargetArray[i] = (bool)boidsList[i].target;
//                     // if ((bool)boidsList[i].target)
//                     //     targetPositionArray[i] = boidsList[i].target.position;
//                 }

//                 CellBoidParallelJob cellBoidParallelJob = new CellBoidParallelJob
//                 {
//                     positionArray = positionArray,
//                     //targetPositionArray = targetPositionArray,
//                     //hasTargetArray = hasTargetArray,
//                     directionArray = directionArray,
//                     accelerationArray = accelerationArray,
//                     velocityArray = velocityArray,
//                     viewRadius = viewRadius,
//                     avoidRadius = avoidRadius,
//                     alignWeight = alignWeight,
//                     cohesionWeight = cohesionWeight,
//                     seperateWeight = seperateWeight,
//                     maxSpeed = maxSpeed,
//                     maxSteerForce = maxSteerForce,
//                     targetWeight = targetWeight
//                 };

//                 JobHandle jobHandle = cellBoidParallelJob.Schedule(Count, 256);
//                 jobHandle.Complete();

//                 for (int i = 0; i < Count; i++)
//                 {
//                     foodNeedsSum += boidsList[i].foodNeeds;
//                     boidsList[i].UpdateBoid(accelerationArray[i]);
//                 }

//                 positionArray.Dispose();
//                 directionArray.Dispose();
//                 accelerationArray.Dispose();
//                 velocityArray.Dispose();
//                 //targetPositionArray.Dispose();
//                 //hasTargetArray.Dispose();
//             }
//         }

//         ecoSystemManager.setfoodDemandFishes(foodNeedsSum);
//     }
// }


// public struct CellBoidParallelJob : IJobParallelFor
// {
//     [ReadOnly] public NativeArray<float3> positionArray;
//     //public NativeArray<float3> targetPositionArray;
//     //public NativeArray<bool> hasTargetArray;
//     [ReadOnly] public NativeArray<float3> directionArray;
//     public NativeArray<float3> accelerationArray;
//     public NativeArray<float3> velocityArray;

//     public float viewRadius;
//     public float avoidRadius;
//     public float alignWeight;
//     public float cohesionWeight;
//     public float seperateWeight;
//     public float maxSpeed;
//     public float maxSteerForce;
//     public float targetWeight;

//     public void Execute(int index)
//     {

//         float viewRadiusSqr = viewRadius * viewRadius;
//         float avoidRadiusSqr = avoidRadius * avoidRadius;
//         int numFlockmates = 0;
//         float3 flockHeading = new float3();
//         float3 flockCentre = new float3();
//         float3 separationHeading = new float3();
//         float3 acceleration = new float3();

//         for (int i = 0; i < positionArray.Length; i++)
//         {
//             if (index != i)
//             {
//                 float3 offset = positionArray[i] - positionArray[index];
//                 float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

//                 if (sqrDst < viewRadiusSqr)
//                 {
//                     numFlockmates += 1;
//                     flockHeading += directionArray[i];
//                     flockCentre += positionArray[i];

//                     if (sqrDst < avoidRadiusSqr)
//                     {
//                         separationHeading -= offset / sqrDst;
//                     }
//                 }
//             }
//         }

//         // if (hasTargetArray[index])
//         // {
//         //     float3 offsetToTarget = targetPositionArray[index] - positionArray[index];
//         //     acceleration = Vector3.ClampMagnitude(math.normalize(offsetToTarget) * maxSpeed - velocityArray[index], maxSteerForce) * targetWeight;
//         // }

//         if (numFlockmates != 0)
//         {
//             flockCentre /= numFlockmates;
//             float3 offsetToFlockmatesCentre = (flockCentre - positionArray[index]);

//             float3 alignmentForce = Vector3.ClampMagnitude(math.normalize(flockHeading) * maxSpeed - velocityArray[index], maxSteerForce) * alignWeight;
//             float3 cohesionForce = Vector3.ClampMagnitude(math.normalize(offsetToFlockmatesCentre) * maxSpeed - velocityArray[index], maxSteerForce) * cohesionWeight;
//             float3 seperationForce = Vector3.ClampMagnitude(math.normalize(separationHeading) * maxSpeed - velocityArray[index], maxSteerForce) * seperateWeight;

//             acceleration += alignmentForce;
//             acceleration += cohesionForce;
//             acceleration += seperationForce;
//         }
//         accelerationArray[index] = acceleration;
//     }
// }