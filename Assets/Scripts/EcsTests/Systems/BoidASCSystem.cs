using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class BoidASCSystem : JobComponentSystem
{
    [BurstCompile]
    struct BoidASCSystemJob : IJobForEach<BoidComponent>
    {


        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<ArchetypeChunk> chunks;

        public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;


        public void Execute(ref BoidComponent boidComponentMain)
        {
            float3 separation = math.float3(0);
            float3 cohesion = math.float3(0);
            float3 alignment = math.float3(0);

            for (int chunksIndex = 0; chunksIndex < chunks.Length; chunksIndex++)
            {
                NativeArray<LocalToWorld> localToWorldSlave = chunks[chunksIndex].GetNativeArray(localToWorldType);

                for (int i = 0; i < chunks[chunksIndex].Count; i++)
                {
                    separation += localToWorldSlave[i].Position;
                    cohesion += localToWorldSlave[i].Position;
                    alignment += localToWorldSlave[i].Forward;
                    
                }
            }

            boidComponentMain.separation = separation;
            boidComponentMain.cohesion = cohesion;
            boidComponentMain.alignment = alignment;
            // boidComponentMain.boidLength = chunks.Length;

        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        EntityQuery entityQuery = GetEntityQuery(typeof(LocalToWorld));

        var job = new BoidASCSystemJob();
        job.chunks = entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
        job.localToWorldType = GetArchetypeChunkComponentType<LocalToWorld>();


        return job.Schedule(this, inputDependencies);
    }
}