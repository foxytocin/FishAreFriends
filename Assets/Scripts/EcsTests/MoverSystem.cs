using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(MoveSpeedComponent));


        TranslateionJob translateionJob = new TranslateionJob
        {
            deltaTime = Time.DeltaTime,
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(translateionJob, entityQuery);
        jobHandle.Complete();
    }



    [BurstCompile]
    private struct TranslateionJob : IJobForEach<Translation, MoveSpeedComponent>
    {

    public float deltaTime;

        public void Execute(ref Translation translation, ref MoveSpeedComponent moveSpeedComponent)
        {

            


            // y
            translation.Value.y += moveSpeedComponent.moveSpeedY * deltaTime;

            if (translation.Value.y > 100f)
                moveSpeedComponent.moveSpeedY = -Mathf.Abs(moveSpeedComponent.moveSpeedY);

            if (translation.Value.y < 0f)
                moveSpeedComponent.moveSpeedY = +Mathf.Abs(moveSpeedComponent.moveSpeedY);

            // z
            translation.Value.z += moveSpeedComponent.moveSpeedZ * deltaTime;

            if (translation.Value.z > 250f)
                moveSpeedComponent.moveSpeedZ = -Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            if (translation.Value.z < -250f)
                moveSpeedComponent.moveSpeedZ = +Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            // x
            translation.Value.x += moveSpeedComponent.moveSpeedX * deltaTime;

            if (translation.Value.x > 250f)
                moveSpeedComponent.moveSpeedX = -Mathf.Abs(moveSpeedComponent.moveSpeedX);

            if (translation.Value.x < -250f)
                moveSpeedComponent.moveSpeedX = +Mathf.Abs(moveSpeedComponent.moveSpeedX);
        }
    }
}