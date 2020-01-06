using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class BoidSystem : ComponentSystem
{
    /*
    public float CellRadius = 8.0f;
    public float SeparationWeight = 1.0f;
    public float AlignmentWeight = 1.0f;
    public float TargetWeight = 2.0f;
    public float ObstacleAversionDistance = 30.0f;
    public float MoveSpeed = 25.0f;
    */

    float AlignmentWeight = 0.1f;
    float SeparationWeight = 0.1f;
    float TargetWeight = 0.1f;
    float ObstacleAversionDistance = 0.1f;
    float MoveSpeed = 2f;


    protected override void OnUpdate()
    {

        Entities.ForEach((Entity entity, ref QuadrantEntityComponent quadrantEntity, ref LocalToWorld localToWorld, ref BoidComponent boidComponent) =>
        {

            float deltaTime = Time.DeltaTime;

            // get all keys, that should be considered
            List<int> hashMapKeyList = QuadrantSystem.GetHashKeysForAroundData(localToWorld.Position);

            QuadrantData nearestQuadrantData = default;
            bool foundNearestQuadrantData = false;

            int neighborCount = 0;
            float3 alignment = new float3(0);
            float3 separation = new float3(0);
            float nearestObstacleDistance = 0;

            foreach (int hashMapKey in hashMapKeyList)
            {
                QuadrantData quadrantData;
                neighborCount = 0;

                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (QuadrantSystem.quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
                {
                    do
                    {
                        if (quadrantData.entity.Equals(entity))
                            continue;

                        if (!foundNearestQuadrantData)
                        {
                            nearestQuadrantData = quadrantData;
                            foundNearestQuadrantData = true;
                        }

                        if (math.abs(math.distance(nearestQuadrantData.position, localToWorld.Position)) > math.abs(math.distance(quadrantData.position, localToWorld.Position)))
                        {
                            nearestObstacleDistance = math.distance(nearestQuadrantData.position, localToWorld.Position);
                            nearestQuadrantData = quadrantData;
                            neighborCount = 1;
                            alignment = nearestQuadrantData.position;
                            separation = nearestQuadrantData.position;
                        }


                    } while (QuadrantSystem.quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
            }

            // todo dummy data
            float3 forward = localToWorld.Forward;
            float3 currentPosition = localToWorld.Position;
            float3 nearestObstaclePosition = nearestQuadrantData.position;
            float3 nearestTargetPosition = nearestQuadrantData.position;

            // correct rules
            float3 alignmentResult = AlignmentWeight * math.normalizesafe((alignment / neighborCount) - forward);
            float3 separationResult = SeparationWeight * math.normalizesafe((currentPosition * neighborCount) - separation);
            float3 targetHeading = TargetWeight * math.normalizesafe(nearestTargetPosition - currentPosition);

            float3 obstacleSteering = currentPosition - nearestObstaclePosition;
            float3 avoidObstacleHeading = (nearestObstaclePosition + math.normalizesafe(obstacleSteering) * ObstacleAversionDistance) - currentPosition;

            float3 nearestObstacleDistanceFromRadius = nearestObstacleDistance - ObstacleAversionDistance;
            float3 normalHeading = math.normalizesafe(alignmentResult + separationResult + targetHeading);
            float3 targetForward = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);

            float3 nextHeading = math.normalizesafe(forward + deltaTime * (targetForward - forward));

            // correct placement
            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                    new float3(localToWorld.Position + (nextHeading * MoveSpeed * deltaTime)),
                    quaternion.LookRotationSafe(nextHeading, math.up()),
                    new float3(1.0f, 1.0f, 1.0f))
            };

            /*
            // flipp direction
            if (translation.Value.y > 100f || translation.Value.y < 0f)
                quadrantEntity.velocity.y *= -1;

            if (translation.Value.z > 250f || translation.Value.z < -250f)
                quadrantEntity.velocity.z *= -1;

            if (translation.Value.x > 250f || translation.Value.x < -250f)
                quadrantEntity.velocity.x *= -1;


            // move
            float3 currentPosition = translation.Value;
            float3 targetPosition = currentPosition + quadrantEntity.velocity;

            targetPosition = math.lerp(currentPosition, targetPosition, 0.5f * deltaTime);
            translation.Value = targetPosition;


            // rotate
            float3 lookVector = targetPosition - currentPosition;
            quaternion rotationValue = math.slerp(rotation.Value, quaternion.LookRotationSafe(lookVector, math.up()), 0.75f * deltaTime);
            //quaternion.LookRotationSafe(lookVector, math.up()); //Quaternion.Lerp(Quaternion.LookRotation(lookVector), 0.5f * deltaTime * 3);
            rotation.Value = rotationValue;
            */

        });




    }
}
