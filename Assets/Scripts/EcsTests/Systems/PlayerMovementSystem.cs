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
        float3 forward;
        float3 newTranslation;


    float3 lookW = new float3(200, 40, 200);


    protected override void OnUpdate()
    {
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



        });
    }
}