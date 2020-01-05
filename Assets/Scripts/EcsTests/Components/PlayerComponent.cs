using Unity.Entities;
using Unity.Mathematics;

public struct PlayerComponent : IComponentData
{
    public float speed;
    public float3 postion;
    public float3 velocity;
    public float3 acceleration;
}