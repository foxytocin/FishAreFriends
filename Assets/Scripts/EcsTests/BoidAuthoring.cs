using UnityEngine;
using Unity.Entities;

public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MoveSpeedComponent { });
        dstManager.AddComponentData(entity, new QuadrantEntity { dummy = 1 });
    }
}
