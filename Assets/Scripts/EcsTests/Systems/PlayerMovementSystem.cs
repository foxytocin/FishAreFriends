using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class PlayerMovementSystem : ComponentSystem
{
    // Key events
    bool upKeyPressed = false;
    bool downKeyPressed = false;
    bool leftKeyPressed = false;
    bool rightKeyPressed = false;

    float AlignmentWeight = 1f;
    float SeparationWeight = 1f;
    float TargetWeight = 1f;
    float MoveSpeed = 3f;
    float ObstacleAversionDistance = 1;



    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        EntityQuery entityQuery = GetEntityQuery(typeof(InputComponent), typeof(PlayerComponent), typeof(LocalToWorld));

        Entities.ForEach((Entity entity, ref InputComponent inputComponent, ref PlayerComponent playerComponent, ref LocalToWorld localToWorld) =>
        {

            // key ups
            if (Input.GetKeyDown(KeyCode.A))
                leftKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.D))
                rightKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.W))
                upKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.S))
                downKeyPressed = true;

            // key downs
            if (Input.GetKeyUp(KeyCode.A))
                leftKeyPressed = false;
            if (Input.GetKeyUp(KeyCode.D))
                rightKeyPressed = false;
            if (Input.GetKeyUp(KeyCode.W))
                upKeyPressed = false;
            if (Input.GetKeyUp(KeyCode.S))
                downKeyPressed = false;


            if (leftKeyPressed)
                playerComponent.velocity = new float3(1, 0, 0) * playerComponent.speed;
            if (rightKeyPressed)
                playerComponent.velocity = new float3(1, 0, 0) * playerComponent.speed;
            if (upKeyPressed)
                playerComponent.velocity = new float3(0, 1, 0) * playerComponent.speed;
            if (downKeyPressed)
                playerComponent.velocity = new float3(0, 1, 0) * playerComponent.speed;

            float3 forward = localToWorld.Forward;
            float3 currentPosition = localToWorld.Position;

            /*
            var cellIndex = cellIndices[entityInQueryIndex];
            var neighborCount = cellCount[cellIndex];
            var alignment = cellAlignment[cellIndex];
            var separation = cellSeparation[cellIndex];
            var nearestObstacleDistance = cellObstacleDistance[cellIndex];
            var nearestObstaclePositionIndex = cellObstaclePositionIndex[cellIndex];
            var nearestTargetPositionIndex = cellTargetPositionIndex[cellIndex];
            var nearestObstaclePosition = copyObstaclePositions[nearestObstaclePositionIndex];
            var nearestTargetPosition = copyTargetPositions[nearestTargetPositionIndex];
            */

            var neighborCount = 1;
            var alignment = new float3(1, 1, 1);
            var separation = new float3(0, 0, 0);
            var nearestObstacleDistance = new float3(1, 1, 1);
            var nearestTargetPositionIndex = new float3(1, 1, 1);
            var nearestObstaclePosition = currentPosition + new float3(-1, -1, -1);
            var nearestTargetPosition = currentPosition + new float3(1, 1, 1);


            var alignmentResult = AlignmentWeight * math.normalizesafe((alignment / neighborCount) - forward);
            var separationResult = SeparationWeight * math.normalizesafe((currentPosition * neighborCount) - separation);
            var targetHeading = TargetWeight * math.normalizesafe(nearestTargetPosition - currentPosition);

            var obstacleSteering = currentPosition - nearestObstaclePosition;
            var avoidObstacleHeading = (nearestObstaclePosition + math.normalizesafe(obstacleSteering) * ObstacleAversionDistance) - currentPosition;

            var nearestObstacleDistanceFromRadius = nearestObstacleDistance - ObstacleAversionDistance;
            var normalHeading = math.normalizesafe(alignmentResult + separationResult + targetHeading);
            var targetForward = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);

            var nextHeading = math.normalizesafe(forward + deltaTime * (targetForward - forward));

            nextHeading += new float3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                    new float3(localToWorld.Position + (nextHeading * MoveSpeed * deltaTime)),
                    quaternion.LookRotationSafe(nextHeading, math.up()),
                    new float3(1.0f, 1.0f, 1.0f))
            };

            /*
            if (Input.GetKeyDown(KeyCode.Q))
            {
                playerComponent.speed -= 0.5f;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerComponent.speed += 0.5f;
            }

            playerComponent.velocity.z = playerComponent.speed;

            // move
            float3 currentPosition = localToWorld.Position;
            float3 nextHeading = currentPosition + playerComponent.velocity;

            nextHeading = math.lerp(currentPosition, nextHeading, 0.5f * Time.DeltaTime * playerComponent.speed);


            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                            new float3(currentPosition + (nextHeading * playerComponent.speed * Time.DeltaTime)),
                            quaternion.LookRotationSafe(nextHeading, math.up()),
                            new float3(20.0f, 50.0f, 20.0f))
            };
*/


        });
    }
}