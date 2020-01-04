using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Rotation), typeof(MoveSpeedComponent), typeof(LocalToWorld));

        TranslateionJob translateionJob = new TranslateionJob
        {
            deltaTime = Time.DeltaTime,
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(translateionJob, entityQuery);
        jobHandle.Complete();
    }


    [BurstCompile]
    private struct TranslateionJob : IJobForEach<Translation, Rotation, MoveSpeedComponent, LocalToWorld>
    {

    public float deltaTime;

        public void Execute(ref Translation translation, ref Rotation rotation, ref MoveSpeedComponent moveSpeedComponent, ref LocalToWorld localToWorld)
        {
            // Move
            if (translation.Value.y > 100f)
                moveSpeedComponent.moveSpeedY = -Mathf.Abs(moveSpeedComponent.moveSpeedY);

            if (translation.Value.y < 0f)
                moveSpeedComponent.moveSpeedY = +Mathf.Abs(moveSpeedComponent.moveSpeedY);

            if (translation.Value.z > 250f)
                moveSpeedComponent.moveSpeedZ = -Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            if (translation.Value.z < -250f)
                moveSpeedComponent.moveSpeedZ = +Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            if (translation.Value.x > 250f)
                moveSpeedComponent.moveSpeedX = -Mathf.Abs(moveSpeedComponent.moveSpeedX);

            if (translation.Value.x < -250f)
                moveSpeedComponent.moveSpeedX = +Mathf.Abs(moveSpeedComponent.moveSpeedX);

            float3 dir = new float3(moveSpeedComponent.moveSpeedX, moveSpeedComponent.moveSpeedY, moveSpeedComponent.moveSpeedZ);
            float3 currentPosition = translation.Value;
            float3 targetPosition = translation.Value + dir;

            targetPosition = math.lerp(currentPosition, targetPosition, 0.5f * deltaTime);
            translation.Value = targetPosition;



            // Rotate
            float3 lookVector = targetPosition - currentPosition;
            Quaternion rotationValue = Quaternion.LookRotation(lookVector);
            rotation.Value = rotationValue;



        }
    }
}