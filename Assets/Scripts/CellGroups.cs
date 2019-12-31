using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGroups : MonoBehaviour
{

    MapGenerator mapGenerator;

    public bool Debug = true;
    public Vector3Int resolution = new Vector3Int(3, 2, 2);
    private float widthStep;
    private float depthStep;
    private float heightStep;

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
        print(allBoidCells.Count);
    }


    public void RegisterAtCell(Boid boid)
    {
        int cellIndex = GetIndex(boid.position);
        boid.cellIndex = cellIndex;
        allBoidCells[cellIndex].Add(boid);
    }


    int index;
    public int GetIndex(Vector3 pos)
    {
        index = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution.x + (resolution.x * resolution.z * (int)(pos.y / heightStep)));
        index = Mathf.Clamp(index, 0, allBoidCells.Count - 1);
        return index;
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


    void OnDrawGizmos()
    {
        if (Debug)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int z = 0; z < resolution.z; z++)
                {
                    for (int x = 0; x < resolution.x; x++)
                    {
                        Vector3 size = new Vector3(widthStep, heightStep, depthStep);
                        Vector3 position = new Vector3(x * widthStep, y * heightStep, z * depthStep);
                        if (index == (x + z * resolution.x + (resolution.x * resolution.z * y)))
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
        }
    }
}
