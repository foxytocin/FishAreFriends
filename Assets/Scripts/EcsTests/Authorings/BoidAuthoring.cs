using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BoidComponent { });
        dstManager.AddComponentData(entity, new QuadrantEntityComponent { typeOfObject = TypeOfObject.Boid });

        // Remove default transform system components
        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
    }
}