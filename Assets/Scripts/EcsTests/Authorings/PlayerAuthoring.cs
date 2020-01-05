using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;
    [SerializeField] private float speed;
    [SerializeField] private float3 velocity;
    [SerializeField] private float3 acceleration;
    [SerializeField] private float3 position;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new InputComponent
        {
            horizontal = horizontal,
            vertical = vertical
        });

        dstManager.AddComponentData(entity, new PlayerComponent
        {
            speed = speed,
            velocity = velocity,
            acceleration = acceleration,
            postion = position
        });


        // Remove default transform system components
        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);

    }
}