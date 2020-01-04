using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;
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
            Entity instance = entityCommandBuffer.Instantiate(index, spawner.prefab);
            entityCommandBuffer.SetComponent(index, instance, new Translation
            {
                Value = localToWorld.Position + random.NextFloat3Direction() * random.NextFloat() * spawner.maxDistanceFromSpawner,
            });

            entityCommandBuffer.SetComponent(index, instance, new MoveSpeedComponent
            {
                moveSpeedX = random.NextFloat(2f, 10f),
                moveSpeedY = random.NextFloat(2f, 10f),
                moveSpeedZ = random.NextFloat(2f, 10f)
            });
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