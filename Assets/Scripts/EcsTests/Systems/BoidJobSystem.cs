using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class BoidJobSystem : ComponentSystem
{

    NativeArray<float3> positionArray;
    NativeArray<float3> directionArray;
    NativeArray<int> numFlockmatesArray;
    NativeArray<float3> flockHeadingArray;
    NativeArray<float3> flockCentreArray;
    NativeArray<float3> avoidanceHeadingArray;
    NativeMultiHashMap<int, int> quadrantMultiHashMap;
    float viewRadius = 8f;
    float avoidRadius = 3f;

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(LocalToWorld), typeof(BoidComponent));


        int Count = entityQuery.CalculateEntityCount();

        positionArray = new NativeArray<float3>(Count, Allocator.TempJob);
        directionArray = new NativeArray<float3>(Count, Allocator.TempJob);
        numFlockmatesArray = new NativeArray<int>(Count, Allocator.TempJob);
        flockHeadingArray = new NativeArray<float3>(Count, Allocator.TempJob);
        flockCentreArray = new NativeArray<float3>(Count, Allocator.TempJob);
        avoidanceHeadingArray = new NativeArray<float3>(Count, Allocator.TempJob);
        quadrantMultiHashMap = new NativeMultiHashMap<int, int>(Count, Allocator.TempJob);


        // read data
        CopyFromBoidJob copyFromBoidJob = new CopyFromBoidJob
        {
            positionArray = positionArray,
            directionArray = directionArray,
        };

        JobHandle jobHandleCopyFromJob = JobForEachExtensions.Schedule(copyFromBoidJob, entityQuery);
        jobHandleCopyFromJob.Complete();

        // quadrant system
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
        {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        QuadrantSystemJob quadrantSystemJob = new QuadrantSystemJob
        {
            positionArray = positionArray,
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        };

        JobHandle quadrantSystemJobHandle = quadrantSystemJob.Schedule(Count, 100);
        quadrantSystemJobHandle.Complete();


        foreach(float3 pos in positionArray)
        {
            QuadrantSystem.drawBoxesAroundEntities(quadrantMultiHashMap, quadrantCellSize, GetPositionHashMapKey(pos));
        }



        // calculate boid stuff
        BoidASCSystemJob job = new BoidASCSystemJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap,
            positionArray = positionArray,
            directionArray = directionArray,
            numFlockmatesArray = numFlockmatesArray,
            flockHeadingArray = flockHeadingArray,
            flockCentreArray = flockCentreArray,
            avoidanceHeadingArray = avoidanceHeadingArray,
            viewRadius = viewRadius,
            avoidRadius = avoidRadius
        };

        JobHandle jobHandle = job.Schedule(Count, 100);
        jobHandle.Complete();

        positionArray.Dispose();
        directionArray.Dispose();


        CopyToBoidJob copyToBoidJob = new CopyToBoidJob
        {
            numFlockmatesArray = numFlockmatesArray,
            flockHeadingArray = flockHeadingArray,
            flockCentreArray = flockCentreArray,
            avoidanceHeadingArray = avoidanceHeadingArray,
            deltaTime = Time.DeltaTime,
        };

        JobHandle jobHandleCopyToBoidJob = JobForEachExtensions.Schedule(copyToBoidJob, entityQuery);
        jobHandleCopyToBoidJob.Complete();

  
        numFlockmatesArray.Dispose();
        flockHeadingArray.Dispose();
        flockCentreArray.Dispose();
        avoidanceHeadingArray.Dispose();
        quadrantMultiHashMap.Dispose();

    }

    [BurstCompile]
    public struct QuadrantSystemJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, int>.ParallelWriter quadrantMultiHashMap;
        [ReadOnly] public NativeArray<float3> positionArray;

        public void Execute(int index)
        {
            int hashMapKey = GetPositionHashMapKey(positionArray[index]);
            quadrantMultiHashMap.Add(hashMapKey,  index);
        }
    }

    [BurstCompile]
    public struct CopyToBoidJob : IJobForEachWithEntity<LocalToWorld, BoidComponent>
    {
        public NativeArray<int> numFlockmatesArray;
        public NativeArray<float3> flockHeadingArray;
        public NativeArray<float3> flockCentreArray;
        public NativeArray<float3> avoidanceHeadingArray;
        public float deltaTime;


        public void Execute(Entity entity, int index, ref LocalToWorld localToWorld, ref BoidComponent boidComponent)
        {
            float3 forward = localToWorld.Forward;
            float3 currentPosition = localToWorld.Position;

            float3 nearestQuadrantDataObstacle = new float3(0);

            float nearestObstacleDistance = math.distance(nearestQuadrantDataObstacle, localToWorld.Position);
            float3 nearestObstaclePosition = nearestQuadrantDataObstacle;

            float3 nearestTargetPosition = flockCentreArray[index] / numFlockmatesArray[index];
            float3 alignmentResult = boidComponent.AlignmentWeight * math.normalizesafe((flockHeadingArray[index] / numFlockmatesArray[index]) - forward);
            float3 separationResult = boidComponent.SeparationWeight * math.normalizesafe((currentPosition * numFlockmatesArray[index]) - avoidanceHeadingArray[index]);

            //Debug.Log("separationResult: " + separationResult);
            //Debug.Log("alignmentResult: " + alignmentResult);
            //Debug.Log("neighborCount: " + neighborCount);

            float3 targetHeading = boidComponent.TargetWeight * math.normalizesafe(nearestTargetPosition - currentPosition);
            float3 obstacleSteering = currentPosition - nearestObstaclePosition;
            float3 avoidObstacleHeading = (nearestObstaclePosition + math.normalizesafe(obstacleSteering) * boidComponent.ObstacleAversionDistance) - currentPosition;

            float3 nearestObstacleDistanceFromRadius = nearestObstacleDistance - boidComponent.ObstacleAversionDistance;
            float3 normalHeading = math.normalizesafe(alignmentResult + separationResult + targetHeading);
            float3 targetForward = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);

            float3 nextHeading = math.normalizesafe(forward + deltaTime * (targetForward - forward));

   
            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                    new float3(localToWorld.Position + (nextHeading * boidComponent.MoveSpeed * deltaTime)),
                    quaternion.LookRotationSafe(nextHeading, math.up()),
                    new float3(1.0f, 1.0f, 1.0f))
            };
        }
    }


    [BurstCompile]
    public struct CopyFromBoidJob : IJobForEachWithEntity<LocalToWorld, BoidComponent>
    {
        public NativeArray<float3> positionArray;
        public NativeArray<float3> directionArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld, [ReadOnly] ref BoidComponent boidComponent)
        {
            positionArray[index] = localToWorld.Position;
            directionArray[index] = localToWorld.Forward;
        }
    }

    [BurstCompile]
    public struct BoidASCSystemJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeMultiHashMap<int, int> quadrantMultiHashMap;
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
            float3 flockHeading = math.float3(0);
            float3 flockCentre = math.float3(0);
            float3 separationHeading = math.float3(0);
            int numFlockmates = 0;


            NativeArray<int> hashMapKeyArray = new NativeArray<int>(27, Allocator.Temp);
            int c3 = 0;
            for (float x = positionArray[index].x - quadrantCellSize; x < positionArray[index].x + quadrantCellSize + 1; x += quadrantCellSize)
                for (float y = positionArray[index].y - quadrantCellSize; y < positionArray[index].y + quadrantCellSize + 1; y += quadrantCellSize)
                    for (float z = positionArray[index].z - quadrantCellSize; z < positionArray[index].z + quadrantCellSize + 1; z += quadrantCellSize)
                    {
                        hashMapKeyArray[c3] = GetXYZHashMapKey(x, y, z);
                    }
            
            for (int c2 = 0; c2 < hashMapKeyArray.Length; c2++)
            {
                int outIndex;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKeyArray[c2], out outIndex, out nativeMultiHashMapIterator))
                {
                    do
                    {
                        if (index != outIndex)
                        {
                            float3 offset = positionArray[outIndex] - positionArray[index];
                            float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                            if (sqrDst < viewRadiusSqr)
                            {
                                numFlockmates += 1;
                                flockHeading += directionArray[outIndex];
                                flockCentre += positionArray[outIndex];

                                if (sqrDst < avoidRadiusSqr)
                                {
                                    separationHeading -= offset / sqrDst;
                                }
                            }
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out outIndex, ref nativeMultiHashMapIterator));
                }
            }

            hashMapKeyArray.Dispose();

            numFlockmatesArray[index] = numFlockmates;
            flockHeadingArray[index] = flockHeading;
            flockCentreArray[index] = flockCentre;
            avoidanceHeadingArray[index] = separationHeading;
        }
    }



    public const int quadrantXMultiplier = 569;
    public const int quadrantYMultiplier = 3658;
    public const int quadrantZMultiplier = 7896;
    public const int quadrantCellSize = 20;

    private static int GetPositionHashMapKey(float3 position)
    {
        return (int)((quadrantXMultiplier * math.floor(position.x / quadrantCellSize)) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)) + (quadrantZMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    private static int GetXYZHashMapKey(float x, float y, float z)
    {
        return (int)((quadrantXMultiplier * math.floor(x / quadrantCellSize)) + (quadrantYMultiplier * math.floor(y / quadrantCellSize)) + (quadrantZMultiplier * math.floor(z / quadrantCellSize)));
    }

   





}