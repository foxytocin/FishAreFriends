using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public struct QuadrantEntity : IComponentData {
    public int dummy;
}

public struct QuadrantData {
    public Entity entity;
    public QuadrantEntity quadrantEntity;
}


public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 1000;
    public const int quadrantZMultiplier = 3000;
    public const int quadrantCellSize = 50;

    private static int GetPositionHashMapKey(float3 position) {
        //return (int) (math.floor(position.x / quadratCellSize) + (quadrantZMultiplier * math.floor(position.z / quadratCellSize)));
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize) + (quadrantZMultiplier * math.floor(position.z / quadrantCellSize))));
       
        //playerCell = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(pos.y / heightStep)));
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey)
    {
        QuadrantData quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
        {
            do
            {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }

        return count;
    }

    protected override void OnCreate()
    {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate() {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));

        quadrantMultiHashMap.Clear();
        if(entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity) {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();

        }


        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();


        for(int x = -250; x < 250; x += quadrantCellSize) {
            for (int y = 0; y < 150; y += quadrantCellSize) {
                for (int z = -250; z < 250; z += quadrantCellSize) {
                    float3 position = new float3(x, y, z);
                    if(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(position)) > 0)
                    {
                        DrawDebugLines(position);
                    }
                    
                }
             }
        }

    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            Debug.Log(hashMapKey);
            quadrantMultiHashMap.Add(hashMapKey,
                new QuadrantData {
                    entity = entity,
                    quadrantEntity = quadrantEntity,
            });
        }
    }



    private void DrawDebugLines(float3 position)
    {
        Vector3 lowerLeft = new Vector3(quadrantCellSize * math.floor(position.x / quadrantCellSize), quadrantCellSize * math.floor(position.y / quadrantCellSize), quadrantCellSize * math.floor(position.z / quadrantCellSize));
        // bottom
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);

        // wall
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1, +0) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize, lowerLeft + new Vector3(+0, +1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadrantCellSize);

        // top
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +0) * quadrantCellSize, lowerLeft + new Vector3(+0, +1, +1) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+1, +1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadrantCellSize);

    }

} 
