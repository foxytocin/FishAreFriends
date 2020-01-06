/*using Unity.Burst;
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
           
        }
    }

}
*/