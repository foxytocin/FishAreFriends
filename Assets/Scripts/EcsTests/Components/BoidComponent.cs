using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WriteGroup(typeof(LocalToWorld))]
public struct BoidComponent : IComponentData
{
    public float3 cohesion;
    public float3 alignment;
    public float3 separation;
}
