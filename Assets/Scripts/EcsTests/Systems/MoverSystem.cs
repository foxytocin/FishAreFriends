using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Rotation), typeof(BoidComponent));

        TranslateionJob translateionJob = new TranslateionJob
        {
            deltaTime = Time.DeltaTime,
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(translateionJob, entityQuery);
        jobHandle.Complete();
    }


    [BurstCompile]
    private struct TranslateionJob : IJobForEach<Translation, Rotation, BoidComponent>
    {

        public float deltaTime;

        public void Execute(ref Translation translation, ref Rotation rotation, ref BoidComponent boidComponent)
        {
            // flipp direction
            if (translation.Value.y > 100f || translation.Value.y < 0f)
                boidComponent.velocity.y *= -1;

            if (translation.Value.z > 250f || translation.Value.z < -250f)
                boidComponent.velocity.z *= -1;

            if (translation.Value.x > 250f || translation.Value.x < -250f)
                boidComponent.velocity.x *= -1;


            // move
            float3 currentPosition = translation.Value;
            float3 targetPosition = currentPosition + boidComponent.velocity;

            targetPosition = math.lerp(currentPosition, targetPosition, 0.5f * deltaTime);
            translation.Value = targetPosition;


            // rotate
            float3 lookVector = targetPosition - currentPosition;
            quaternion rotationValue = math.slerp(rotation.Value, quaternion.LookRotationSafe(lookVector, math.up()), 0.75f * deltaTime);   
            //quaternion.LookRotationSafe(lookVector, math.up()); //Quaternion.Lerp(Quaternion.LookRotation(lookVector), 0.5f * deltaTime * 3);
            rotation.Value = rotationValue;
        }
    }
}