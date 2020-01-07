using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class QuadrantSystem 
{
    /////////////////////// 
    /// Debug Functions ///
    /////////////////////// 

    public static int GetEntityCountInHashMap(NativeMultiHashMap<int, int> quadrantMultiHashMap, int hashMapKey) {
        int quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
        {
            do
            {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }

        return count;
    }



    /*
     * Use this function to draw box/boxes around an entitiy.
     */
    public static void drawBoxesAroundEntities(NativeMultiHashMap<int, int> quadrantMultiHashMap, int quadrantCellSize, int hashKey)
    {
        for (int x = -250; x < 250; x += quadrantCellSize)
            for (int y = 0; y < 150; y += quadrantCellSize)
                for (int z = -250; z < 250; z += quadrantCellSize)
                {
                    float3 position = new float3(x, y, z);
                    if (GetEntityCountInHashMap(quadrantMultiHashMap, hashKey) > 0)
                    {
                        // you can trigger here between
                        // - show one box around the entity
                        // - show all boxes that will later be used for neighbour searching

                        //DrawDebugBoxAndBoxesAround(position);
                        DrawDebugBox(position, quadrantCellSize);
                    }
                }
    }

    /*
     * This methode returns you a list<int> with all keys around your actual position
     */
    public static List<int> GetHashKeysForAroundData(float3 position, int quadrantCellSize, int hashKey)
    {
        List<int> list = new List<int>();
        for (float x = position.x - quadrantCellSize; x < position.x + quadrantCellSize + 1; x += quadrantCellSize)
            for (float y = position.y - quadrantCellSize; y < position.y + quadrantCellSize + 1; y += quadrantCellSize)
                for (float z = position.z - quadrantCellSize; z < position.z + quadrantCellSize + 1; z += quadrantCellSize)
                {
                    list.Add(hashKey);
                }
        return list;
    }






    /*
    * Use this methode to test the hashCode function. If two Cubes are shown in the SceneView, 
    * you have duplicate hashcodes for different positions
    */
    public static void checkDuplicateHashCodes(int quadrantCellSize, int hashKey)
    {
        Dictionary<int, float3> map = new Dictionary<int, float3>();

        for (int x = -250; x < 250; x += quadrantCellSize)
        {
            for (int y = 0; y < 150; y += quadrantCellSize)
            {
                for (int z = -250; z < 250; z += quadrantCellSize)
                {
                    float3 position = new float3(x, y, z);
                    if (map.ContainsKey(hashKey))
                    {
                        DrawDebugBox(position, quadrantCellSize);

                        float3 outPosition;
                        map.TryGetValue(hashKey, out outPosition);
                        DrawDebugBox(outPosition, quadrantCellSize);
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
    public static void DrawDebugBoxAndBoxesAround(float3 position, int quadrantCellSize)
    {
        DrawDebugBox(position, quadrantCellSize);

        for (float x = position.x - quadrantCellSize; x < position.x + quadrantCellSize + 1; x += quadrantCellSize)
            for (float y = position.y - quadrantCellSize; y < position.y + quadrantCellSize + 1; y += quadrantCellSize)
                for (float z = position.z - quadrantCellSize; z < position.z + quadrantCellSize + 1; z += quadrantCellSize)
                {
                    float3 tempPosition = new float3(x, y, z);
                    DrawDebugBox(tempPosition, quadrantCellSize);
                }
    }

    /*
     * This methode draws a box with lines in SceneView
     */
    public static void DrawDebugBox(float3 position, int quadrantCellSize)
    {
        float3 lowerLeft = new float3(quadrantCellSize * math.floor(position.x / quadrantCellSize), quadrantCellSize * math.floor(position.y / quadrantCellSize), quadrantCellSize * math.floor(position.z / quadrantCellSize));

        // bottom
        Debug.DrawLine(lowerLeft, lowerLeft + new float3(+1, +0, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new float3(+0, +0, +1) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new float3(+1, +0, +0) * quadrantCellSize, lowerLeft + new float3(+1, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new float3(+0, +0, +1) * quadrantCellSize, lowerLeft + new float3(+1, +0, +1) * quadrantCellSize);

        // wall
        Debug.DrawLine(lowerLeft, lowerLeft + new float3(+0, +1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new float3(+1, +0, +0) * quadrantCellSize, lowerLeft + new float3(+1, +1, +0) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new float3(+0, +0, +1) * quadrantCellSize, lowerLeft + new float3(+0, +1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new float3(+1, +0, +1) * quadrantCellSize, lowerLeft + new float3(+1, +1, +1) * quadrantCellSize);

        // top
        Debug.DrawLine(lowerLeft + new float3(+0, +1, +0) * quadrantCellSize, lowerLeft + new float3(+1, +1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new float3(+0, +1, +0) * quadrantCellSize, lowerLeft + new float3(+0, +1, +1) * quadrantCellSize);

        Debug.DrawLine(lowerLeft + new float3(+1, +1, +0) * quadrantCellSize, lowerLeft + new float3(+1, +1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new float3(+0, +1, +1) * quadrantCellSize, lowerLeft + new float3(+1, +1, +1) * quadrantCellSize);
    }

}
