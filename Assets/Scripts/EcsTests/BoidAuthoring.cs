using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MoveSpeedComponent());
        dstManager.AddComponentData(entity, new QuadrantEntity { dummy = 1 });
    }
}
