using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;

public class RotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        /*
        EntityQuery entityQuery = GetEntityQuery(typeof(Rotate), typeof(RotationEulerXYZ));

        TranslateionJob translateionJob = new TranslateionJob
        {
            deltaTime = Time.DeltaTime,
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(translateionJob, entityQuery);
        jobHandle.Complete();
        */

    }

    /*
    [BurstCompile]
    private struct TranslateionJob : IJobForEach<Rotate, RotationEulerXYZ>
    {
        
        public float deltaTime;

        public void Execute(ref Rotate rotate, ref RotationEulerXYZ euler)
        {
            euler.Value.y += rotate.radiansPerSeconds * deltaTime;
        }
        
    }
    */
}
