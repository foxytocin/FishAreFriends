using Unity.Entities;

public struct SpawnerA : IComponentData
{
    public Entity prefab;
    public float maxDistanceFromSpawner;
    public float secondsBetweenSpawns;
    public float secondsToNextSpawn;
}