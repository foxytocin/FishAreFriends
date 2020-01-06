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

    float AlignmentWeight = 2f;
    float SeparationWeight = 1f;
    float TargetWeight = 3f;
    float ObstacleAversionDistance = 3f;
    float MoveSpeed = 3f;

    float viewWidth = 8f;


    protected override void OnUpdate()
    {

        Entities.ForEach((Entity entity, ref QuadrantEntityComponent quadrantEntity, ref LocalToWorld localToWorld, ref BoidComponent boidComponent) =>
        {

            float deltaTime = Time.DeltaTime;

            // get all keys, that should be considered
            List<int> hashMapKeyList = QuadrantSystem.GetHashKeysForAroundData(localToWorld.Position);

            QuadrantData nearestQuadrantDataBoid = default;
            bool foundNearestQuadrantDataBoid = false;

            QuadrantData nearestQuadrantDataObstacle = default;
            bool foundNearestQuadrantDataObstacle = false;

            int neighborCount = 0;
            float3 alignment = math.float3(0);
            float3 separation = math.float3(0);
            float3 cohesion = math.float3(0);


            foreach (int hashMapKey in hashMapKeyList)
            {
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (QuadrantSystem.quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
                {
                    do
                    {
                        if (quadrantData.entity.Equals(entity))
                            continue;

                        // nearest boid searching
                        if (!foundNearestQuadrantDataBoid && quadrantData.typeOfObject.Equals(TypeOfObject.Boid))
                        {
                            nearestQuadrantDataBoid = quadrantData;
                            foundNearestQuadrantDataBoid = true;
                        }

                        if (math.abs(math.distance(nearestQuadrantDataBoid.position, localToWorld.Position)) > math.abs(math.distance(quadrantData.position, localToWorld.Position)) && quadrantData.typeOfObject.Equals(TypeOfObject.Boid))
                            nearestQuadrantDataBoid = quadrantData;


                        // nearest obstacle searching
                        if (!foundNearestQuadrantDataObstacle && quadrantData.typeOfObject.Equals(TypeOfObject.Obstacle))
                        {
                            nearestQuadrantDataObstacle = quadrantData;
                            foundNearestQuadrantDataObstacle = true;
                        }

                        float distance = math.abs(math.distance(quadrantData.position, localToWorld.Position));

                        if (math.abs(math.distance(nearestQuadrantDataObstacle.position, localToWorld.Position)) > distance && quadrantData.typeOfObject.Equals(TypeOfObject.Obstacle))
                            nearestQuadrantDataObstacle = quadrantData;


                        if (quadrantData.typeOfObject.Equals(TypeOfObject.Boid))
                        {
                            if(distance <= viewWidth){

                            
                                neighborCount++;
                                alignment += quadrantData.forward;
                                separation += quadrantData.position;
                                cohesion += quadrantData.position;

                                //Debug.Log("quadrantData.forward: " + quadrantData.forward);
                                //Debug.Log("quadrantData.position: " + quadrantData.position);

                            }
                        }



                    } while (QuadrantSystem.quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
            }


            // temporarily storing the values for code readability
            float3 forward = localToWorld.Forward;
            float3 currentPosition = localToWorld.Position;            
            
            
            float nearestObstacleDistance = math.distance(nearestQuadrantDataObstacle.position, localToWorld.Position);



            float3 nearestObstaclePosition = nearestQuadrantDataObstacle.position;
            //float3 nearestTargetPosition = math.float3(0,3,200);
            float3 nearestTargetPosition = cohesion / neighborCount;

            float3 alignmentResult = AlignmentWeight * math.normalizesafe((alignment / neighborCount) - forward);
            float3 separationResult = SeparationWeight * math.normalizesafe((currentPosition * neighborCount) - separation);

            //Debug.Log("separationResult: " + separationResult);
            //Debug.Log("alignmentResult: " + alignmentResult);
            //Debug.Log("neighborCount: " + neighborCount);

            float3 targetHeading = TargetWeight * math.normalizesafe(nearestTargetPosition - currentPosition);
            var obstacleSteering = currentPosition - nearestObstaclePosition;
            var avoidObstacleHeading = (nearestObstaclePosition + math.normalizesafe(obstacleSteering) * ObstacleAversionDistance) - currentPosition;

            var nearestObstacleDistanceFromRadius = nearestObstacleDistance - ObstacleAversionDistance;
            var normalHeading = math.normalizesafe(alignmentResult + separationResult + targetHeading);
            var targetForward = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);

            var nextHeading = math.normalizesafe(forward + deltaTime * (targetForward - forward));


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
