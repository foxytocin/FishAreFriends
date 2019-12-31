using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGroups : MonoBehaviour
{

    MapGenerator mapGenerator;

    public bool Debug = true;
    public int resolution = 3;
    private float widthStep;
    private float depthStep;
    private float heightStep;

    public List<List<Boid>> allBoidCells = new List<List<Boid>>();



    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        widthStep = mapGenerator.mapSize.x / resolution;
        heightStep = mapGenerator.mapSize.y / resolution;
        depthStep = mapGenerator.mapSize.z / resolution;

        for (int i = 0; i < ((resolution * resolution * resolution)); i++)
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
        index = ((int)(pos.x / widthStep) + (int)(pos.z / depthStep) * resolution + (resolution * resolution * (int)(pos.y / heightStep)));
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
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        Vector3 size = new Vector3(widthStep, heightStep, depthStep);
                        Vector3 position = new Vector3(x * widthStep, y * heightStep, z * depthStep);
                        if (index == (x + z * resolution + (resolution * resolution * y)))
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
