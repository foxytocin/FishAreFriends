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
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveSpeedComponent),
            typeof(QuadrantEntity)
        );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(10000, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);
       

        for( int i= 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];

            entityManager.SetComponentData(
                entity, 
                new Translation {
                    Value = new Unity.Mathematics
                    .float3(
                        Random.Range(-250, 250), 
                        Random.Range(0, 5),
                        Random.Range(-250, 250))
                });

            entityManager.SetComponentData(entity, new MoveSpeedComponent {
                moveSpeedX = Random.Range(1f, 3f),
                moveSpeedY = Random.Range(1f, 3f),
                moveSpeedZ = Random.Range(1f, 3f)
            });

            entityManager.SetComponentData(entity, new QuadrantEntity
            {
                dummy = 1
            });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = mat
            });


        }


        entityArray.Dispose();

    }

}
