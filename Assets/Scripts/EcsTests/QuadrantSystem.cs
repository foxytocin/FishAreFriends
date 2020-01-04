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
    public float3 position;
    //public QuadrantEntity quadrantEntity;
}


public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantXMultiplier = 569;
    public const int quadrantYMultiplier = 3658;
    public const int quadrantZMultiplier = 7896;
    public const int quadrantCellSize = 20;

    private static int GetPositionHashMapKey(float3 position) {
        return (int) ((quadrantXMultiplier* math.floor(position.x / quadrantCellSize)) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)) + (quadrantZMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    private static int GetXYZHashMapKey(float x, float y, float z)
    {
        return (int)((quadrantXMultiplier * math.floor(x / quadrantCellSize)) + (quadrantYMultiplier * math.floor(y / quadrantCellSize)) + (quadrantZMultiplier * math.floor(z / quadrantCellSize)));
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


        drawBoxesAroundEntities();

        /*
        checkDuplicateHashCodes();
        */


    }

    /*
     * Use this function to draw box/boxes around an entitiy.
     */
    private void drawBoxesAroundEntities()
    {
        for (int x = -250; x < 250; x += quadrantCellSize)
            for (int y = 0; y < 150; y += quadrantCellSize)
                for (int z = -250; z < 250; z += quadrantCellSize)
                {
                    float3 position = new float3(x, y, z);
                    if (GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(position)) > 0)
                    {
                        // you can trigger here between
                        // - show one box around the entity
                        // - show all boxes that will later be used for neighbour searching

                        //DrawDebugBoxAndBoxesAround(position);
                        DrawDebugBox(position);
                    }
                }
    }

    /*
     * This methode returns you a list<int> with all keys around your actual position
     */ 
    public static List<int> GetHashKeysForAroundData(float3 position)
    {
        List<int> list = new List<int>();
        for (float x = position.x - quadrantCellSize; x < position.x + quadrantCellSize + 1; x += quadrantCellSize)
            for (float y = position.y - quadrantCellSize; y < position.y + quadrantCellSize + 1; y += quadrantCellSize)
                for (float z = position.z - quadrantCellSize; z < position.z + quadrantCellSize + 1; z += quadrantCellSize)
                {
                    list.Add(GetXYZHashMapKey(x, y, z));
                }
        return list;
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
                    position = translation.Value,
                    //quadrantEntity = quadrantEntity,
            });
        }
    }

    /////////////////////// 
    /// Debug Functions ///
    /////////////////////// 

    /*
    * Use this methode to test the hashCode function. If two Cubes are shown in the SceneView, 
    * you have duplicate hashcodes for different positions
    */
    private void checkDuplicateHashCodes()
    {
        Dictionary<int, float3> map = new Dictionary<int, float3>();

        for (int x = -250; x < 250; x += quadrantCellSize)
        {
            for (int y = 0; y < 150; y += quadrantCellSize)
            {
                for (int z = -250; z < 250; z += quadrantCellSize)
                {
                    float3 position = new float3(x, y, z);
                    int hashKey = GetPositionHashMapKey(position);
                    if (map.ContainsKey(hashKey))
                    {
                        DrawDebugBox(position);

                        float3 outPosition;
                        map.TryGetValue(hashKey, out outPosition);
                        DrawDebugBox(outPosition);
                    }
                    else
                    {
                        map.Add(hashKey, position);
                    }

                }
            }
        }

        Debug.Log("No duplicates found");
    }


    /*
     * Use this function to draw a box around a position according to the quadrantCellSize
     */
    private void DrawDebugBoxAndBoxesAround(float3 position)
    {
        DrawDebugBox(position);

        for(float x = position.x - quadrantCellSize; x < position.x + quadrantCellSize +1; x+= quadrantCellSize)
            for (float y = position.y - quadrantCellSize; y < position.y + quadrantCellSize +1; y += quadrantCellSize)
                for (float z = position.z - quadrantCellSize; z < position.z + quadrantCellSize +1; z += quadrantCellSize)
                {
                    float3 tempPosition = new float3(x, y, z);
                    DrawDebugBox(tempPosition);
                }
    }

    /*
     * This methode draws a box with lines in SceneView
     */ 
    private void DrawDebugBox(float3 position)
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
