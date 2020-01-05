using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class PlayerMovementSystem : ComponentSystem
{
    float3 velocity;
    float speed = 20f;


    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(InputComponent), typeof(Translation));

        velocity.x += Time.DeltaTime * speed * Input.GetAxis("Horizontal");
        velocity.y += Time.DeltaTime * speed * Input.GetAxis("Vertical"); ;

        Entities.ForEach((Entity entity, ref InputComponent inputComponent, ref Translation translation) =>
        {
            translation.Value.x = math.lerp(translation.Value.x, velocity.x, 0.5f * Time.DeltaTime * 10);
            translation.Value.y = math.lerp(translation.Value.y, velocity.y, 0.5f * Time.DeltaTime * 10);
        });
    }
}