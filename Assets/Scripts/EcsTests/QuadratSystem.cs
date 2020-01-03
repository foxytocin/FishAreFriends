using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class QuadratSystem : ComponentSystem
{
    private const int quadrantZMultiplier = 1000;
    private const int quadratCellSize = 50;

    private static int GetPositionHashMapKey(float3 position) {
        return (int) (math.floor(position.x / quadratCellSize) + (quadrantZMultiplier * math.floor(position.z / quadratCellSize)));

        //playerCell = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(pos.y / heightStep)));
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, Entity> quadrantMultiHashMap, int hashMapKey)
    {
        Entity entity;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out entity, out nativeMultiHashMapIterator))
        {
            do
            {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out entity, ref nativeMultiHashMapIterator));
        }

        return count;
    }

    protected override void OnUpdate() {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation));
        NativeMultiHashMap<int, Entity> quadrantMultiHashMap = new NativeMultiHashMap<int, Entity>(entityQuery.CalculateEntityCount(), Allocator.TempJob);

        /*
        Entities.ForEach((Entity entity, ref Translation translation) => {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            //Debug.Log(hashMapKey);
            quadrantMultiHashMap.Add(hashMapKey, entity);


        
            DrawDebugLines(translation.Value);
        });
        */

        

        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        };

        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();


        //Vector3 position = new Vector3(0, 3, 0);
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(position)));

        quadrantMultiHashMap.Dispose();


    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation>
    {
        public NativeMultiHashMap<int, Entity>.ParallelWriter quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, entity);
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
