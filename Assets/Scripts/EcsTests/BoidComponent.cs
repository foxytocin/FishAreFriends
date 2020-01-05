using Unity.Entities;
using Unity.Mathematics;

public struct BoidComponent : IComponentData
{
    public float3 velocity;
    public float3 targetPosition;
}
