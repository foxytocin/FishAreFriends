using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float maxDistanceFromSpawner;
    [SerializeField] private float spawnRate;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SpawnerA
        {
            prefab = conversionSystem.GetPrimaryEntity(prefab),
            maxDistanceFromSpawner = maxDistanceFromSpawner,
            secondsBetweenSpawns = 1 / spawnRate,
            secondsToNextSpawn = 0
        });
    }
}