using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
public class PlayerMovementSystem : ComponentSystem
{
    // Key events
    bool upKeyPressed = false;
    bool downKeyPressed = false;
    bool leftKeyPressed = false;
    bool rightKeyPressed = false;
    float3 velocity;
    float speed = 20f;


    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(InputComponent), typeof(Translation));

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
            velocity.x -= Time.DeltaTime * speed;
        if (rightKeyPressed)
            velocity.x += Time.DeltaTime * speed;
        if (upKeyPressed)
            velocity.y += Time.DeltaTime * speed;
        if (downKeyPressed)
            velocity.y -= Time.DeltaTime * speed;


        PlayerMovementJob playerMovementJob = new PlayerMovementJob
        {
            deltaTime = Time.DeltaTime,
            velocity = velocity
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(playerMovementJob, entityQuery);
        jobHandle.Complete();

    }


    [BurstCompile]
    private struct PlayerMovementJob : IJobForEach<InputComponent, Translation>
    {
        public float deltaTime;
        public float3 velocity;

        public void Execute(ref InputComponent input, ref Translation translation)
        {
            translation.Value.x = math.lerp(translation.Value.x, velocity.x, 0.5f * deltaTime * 10);
            translation.Value.y = math.lerp(translation.Value.y, velocity.y, 0.5f * deltaTime * 10);
        }
    }
}