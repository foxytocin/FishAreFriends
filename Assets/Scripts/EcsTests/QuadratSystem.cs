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


public class QuadratSystem : ComponentSystem
{
    private static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    private const int quadrantZMultiplier = 1000;
    private const int quadratCellSize = 50;

    private static int GetPositionHashMapKey(float3 position) {
        return (int) (math.floor(position.x / quadratCellSize) + (quadrantZMultiplier * math.floor(position.z / quadratCellSize)));

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


        //Vector3 position = new Vector3(0, 3, 0);
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(position)));



    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey,
                new QuadrantData {
                    entity = entity,
                    quadrantEntity = quadrantEntity,
            });
        }
    }



    private void DrawDebugLines(float3 position)
    {
        Vector3 lowerLeft = new Vector3(quadratCellSize * math.floor(position.x / quadratCellSize), quadratCellSize * math.floor(position.y / quadratCellSize), quadratCellSize * math.floor(position.z / quadratCellSize));
        // bottom
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0, +0) * quadratCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +0, +1) * quadratCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadratCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadratCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadratCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadratCellSize);

        // wall
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1, +0) * quadratCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadratCellSize, lowerLeft + new Vector3(+1, +1, +0) * quadratCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadratCellSize, lowerLeft + new Vector3(+0, +1, +1) * quadratCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +1) * quadratCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadratCellSize);

        // top
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +0) * quadratCellSize, lowerLeft + new Vector3(+1, +1, +0) * quadratCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +0) * quadratCellSize, lowerLeft + new Vector3(+0, +1, +1) * quadratCellSize);

        Debug.DrawLine(lowerLeft + new Vector3(+1, +1, +0) * quadratCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadratCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1, +1) * quadratCellSize, lowerLeft + new Vector3(+1, +1, +1) * quadratCellSize);

    }

} 
