using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

/*

public class BoidASCSystem : JobComponentSystem
{
    [BurstCompile]
    struct BoidASCSystemJob : IJobForEach<BoidMasterComponent>
    {


        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<ArchetypeChunk> chunks;


        public ArchetypeChunkComponentType<BoidSlaveComponent> boidSlaveComponentType;


        public void Execute(ref BoidMasterComponent boidMasterComponent)
        {
            float3 separation = math.float3(0);
            float3 cohesion = math.float3(0);
            float3 alignment = math.float3(0);

            for (int chunksIndex = 0; chunksIndex < chunks.Length; chunksIndex++)
            {
                NativeArray<BoidSlaveComponent> boidSlaveComponent = chunks[chunksIndex].GetNativeArray(boidSlaveComponentType);

                for (int i = 0; i < chunks[chunksIndex].Count; i++)
                {
                    if (math.distance(boidSlaveComponent[i].position, boidMasterComponent.position) > 5)
                    {

                    }

                    separation += boidSlaveComponent[i].position;
                    cohesion += boidSlaveComponent[i].position;
                    alignment += boidSlaveComponent[i].forward;
                    
                }
            }

            boidMasterComponent.separation = separation;
            boidMasterComponent.cohesion = cohesion;
            boidMasterComponent.alignment = alignment;
            // boidComponentMain.boidLength = chunks.Length;

        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        EntityQuery entityQuery = GetEntityQuery(typeof(BoidSlaveComponent));

        var job = new BoidASCSystemJob();
        job.chunks = entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
        job.boidSlaveComponentType = GetArchetypeChunkComponentType<BoidSlaveComponent>();


        return job.Schedule(this, inputDependencies);
    }
}

    */