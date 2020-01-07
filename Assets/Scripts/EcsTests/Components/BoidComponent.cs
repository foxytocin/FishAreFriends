using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WriteGroup(typeof(LocalToWorld))]
public struct BoidComponent : IComponentData
{
    public float AlignmentWeight;
    public float SeparationWeight;
    public float TargetWeight;
    public float ObstacleAversionDistance;
    public float MoveSpeed;
    public float viewWidth;
}
