using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BoidComponent {
            AlignmentWeight = 1f,
            SeparationWeight = 2f,
            TargetWeight = 3f,
            ObstacleAversionDistance = 3f,
            MoveSpeed = 3f,
            viewWidth = 8f,
        });

        dstManager.AddComponentData(entity, new QuadrantEntityComponent {});

        // Remove default transform system components
        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
    }
}