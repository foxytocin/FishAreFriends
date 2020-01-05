using UnityEngine;
using Unity.Entities;

public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float CellRadius = 8.0f;
    public float SeparationWeight = 1.0f;
    public float AlignmentWeight = 1.0f;
    public float TargetWeight = 2.0f;
    public float ObstacleAversionDistance = 30.0f;
    public float MoveSpeed = 25.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BoidComponent {});
        dstManager.AddComponentData(entity, new QuadrantEntityComponent { typeOfObject = TypeOfObject.Boid });
    }
}