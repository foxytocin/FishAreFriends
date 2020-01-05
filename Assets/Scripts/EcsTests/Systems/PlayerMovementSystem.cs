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

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(InputComponent), typeof(Translation), typeof(PlayerComponent), typeof(LocalToWorld), typeof(Rotation));
        
        Entities.ForEach((Entity entity, ref InputComponent inputComponent, ref Translation translation, ref PlayerComponent playerComponent, ref LocalToWorld localToWorld, ref Rotation rotation) =>
        {
            /*
            playerComponent.velocity.x += Time.DeltaTime * playerComponent.speed * Input.GetAxis("Horizontal");
            playerComponent.velocity.y += Time.DeltaTime * playerComponent.speed * Input.GetAxis("Vertical"); ;
            translation.Value.x = math.lerp(translation.Value.x, playerComponent.velocity.x, 0.5f * Time.DeltaTime * 10);
            translation.Value.y = math.lerp(translation.Value.y, playerComponent.velocity.y, 0.5f * Time.DeltaTime * 10);
            */

            if (Input.GetKeyDown(KeyCode.Q))
            {
                playerComponent.speed -= 0.5f;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerComponent.speed += 0.5f;
            }

            translation.Value += new float3(
                Input.GetAxis("Horizontal") * playerComponent.speed * Time.DeltaTime,
                Input.GetAxis("Vertical") * playerComponent.speed * Time.DeltaTime,
                0
                );

        });
    }
}