using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

public class Testing : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material mat;


    void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;


        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveSpeedComponent)
        );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(10000, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);
       

        for( int i= 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new LevelComponent { level = Random.Range(10, 20) });

            entityManager.SetComponentData(
                entity, 
                new Translation {
                    Value = new Unity.Mathematics
                    .float3(
                        Random.Range(-50, 50), 
                        Random.Range(0, 5), 
                        0)
                });

            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeed = Random.Range(1f, 2f) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = mat
            });
        }


        entityArray.Dispose();

    }

}
