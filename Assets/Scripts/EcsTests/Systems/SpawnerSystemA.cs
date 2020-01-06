using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Time = UnityEngine.Time;

public class SpawnerSystemA : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct SpawnerJob : IJobForEachWithEntity<SpawnerA, LocalToWorld>
    {
        private EntityCommandBuffer.Concurrent entityCommandBuffer;
        private Unity.Mathematics.Random random;
        private readonly float deltaTime;

        public SpawnerJob(EntityCommandBuffer.Concurrent entityCommandBuffer, Random random, float deltaTime)
        {
            this.entityCommandBuffer = entityCommandBuffer;
            this.random = random;
            this.deltaTime = deltaTime;
        }

        public void Execute(Entity entity, int index, ref SpawnerA spawner, ref LocalToWorld localToWorld)
        {
            spawner.secondsToNextSpawn -= deltaTime;
            if (spawner.secondsToNextSpawn >= 0) { return; }

            spawner.secondsToNextSpawn += spawner.secondsBetweenSpawns;
            for (int i = 0; i < 50; i++)
            {
                Entity instance = entityCommandBuffer.Instantiate(index + i, spawner.prefab);

                var dir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
                var pos = dir * spawner.maxDistanceFromSpawner;

                entityCommandBuffer.SetComponent(index + i, instance, new LocalToWorld
                {
                    Value = float4x4.TRS(pos, quaternion.LookRotationSafe(dir, math.up()), new float3(1.0f, 1.0f, 1.0f))

                    //Value = localToWorld.Position + random.NextFloat3Direction() * random.NextFloat() * spawner.maxDistanceFromSpawner,
                });

                // just for filtering
                entityCommandBuffer.SetComponent(index + i, instance, new BoidComponent { });
                entityCommandBuffer.SetComponent(index + i, instance, new QuadrantEntityComponent { });
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var spawnerJob = new SpawnerJob(
            endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
            new Random((uint)UnityEngine.Random.Range(0, int.MaxValue)),
            Time.DeltaTime);

        JobHandle jobHandle = spawnerJob.Schedule(this, inputDeps);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}