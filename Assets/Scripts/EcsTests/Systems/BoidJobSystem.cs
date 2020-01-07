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


        // read data
        CopyFromBoidJob copyFromBoidJob = new CopyFromBoidJob
        {
            positionArray = positionArray,
            directionArray = directionArray,
        };

        JobHandle jobHandleCopyFromJob = JobForEachExtensions.Schedule(copyFromBoidJob, entityQuery);
        jobHandleCopyFromJob.Complete();

        // calculate boid stuff
        BoidASCSystemJob job = new BoidASCSystemJob
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


}