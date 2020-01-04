using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct SpawnerA : IComponentData
{
    public Entity prefab;
    public float moveSpeedX;
    public float moveSpeedY;
    public float moveSpeedZ;
    public float maxDistanceFromSpawner;
    public float secondsBetweenSpawns;
    public float secondsToNextSpawn;
}
