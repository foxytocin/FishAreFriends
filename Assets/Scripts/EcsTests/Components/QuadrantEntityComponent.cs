using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct QuadrantEntityComponent : IComponentData
{
    public TypeOfObject typeOfObject;

    // boid data
    public float3 velocity;
}
