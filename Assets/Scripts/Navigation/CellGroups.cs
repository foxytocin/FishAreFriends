using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class CellGroups : MonoBehaviour
{

    MapGenerator mapGenerator;

    public bool Debug = true;
    public float3 resolution = new float3(3, 2, 2);
    public float widthStep;
    public float depthStep;
    public float heightStep;

    public List<List<Boid>> allBoidCells = new List<List<Boid>>();


    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        widthStep = mapGenerator.mapSize.x / resolution.x;
        heightStep = mapGenerator.mapSize.y / resolution.y;
        depthStep = mapGenerator.mapSize.z / resolution.z;

        for (int i = 0; i < ((resolution.x * resolution.y * resolution.z)); i++)
        {
            allBoidCells.Add(new List<Boid>());
        }
    }


    public void RegisterAtCell(Boid boid)
    {
        int cellIndex = GetIndex(boid.position);
        boid.cellIndex = cellIndex;
        allBoidCells[cellIndex].Add(boid);
    }


    public int GetIndex(float3 pos)
    {
        float index = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(pos.y / heightStep)));
        return (int)(Mathf.Clamp(index, 0, allBoidCells.Count - 1));
    }


    public void CheckCell(Boid boid)
    {
        int oldIndex = boid.cellIndex;
        int newIndex = GetIndex(boid.position);

        if (newIndex != oldIndex)
        {
            boid.cellIndex = newIndex;
            allBoidCells[oldIndex].Remove(boid);
            allBoidCells[newIndex].Add(boid);
        }
    }


    float playerCell;
    public void SetPlayerCell(float3 pos)
    {
        playerCell = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(pos.y / heightStep)));
    }


    void OnDrawGizmosSelected()
    {
        if (Debug)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int z = 0; z < resolution.z; z++)
                {
                    for (int x = 0; x < resolution.x; x++)
                    {
                        float3 size = new float3(widthStep, heightStep, depthStep);
                        float3 position = new float3(x * widthStep, y * heightStep, z * depthStep);
                        if (playerCell == (x + z * resolution.x + (resolution.x * resolution.z * y)))
                        {
                            Gizmos.color = new Color(0, 1, 0, 0.5f);
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                        }
                        Gizmos.DrawCube(position + size / 2, size);
                    }
                }
            }

            int cellCount = 0;
            foreach (List<Boid> boidsList in allBoidCells)
            {
                print("BoidCountinCell "+cellCount+ ": " +boidsList.Count);
                cellCount++;
            }
        }
    }
}
