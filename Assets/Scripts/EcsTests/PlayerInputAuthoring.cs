using UnityEngine;
using Unity.Entities;

public class PlayerInputAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new InputComponent
        {
            horizontal = horizontal,
            vertical = vertical
        });
    }
}