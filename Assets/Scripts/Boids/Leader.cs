using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour
{

    CellGroups cellGroups;
    MapGenerator mapGenerator;
    private List<Boid> swarmList;

    public Color leaderColor1;
    public Color leaderColor2;
    private Material material;

    private void Awake()
    {
        //leaderColor = Random.ColorHSV();
        cellGroups = FindObjectOfType<CellGroups>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        transform.position = mapGenerator.mapSize / 2;
        material = gameObject.GetComponentInChildren<MeshRenderer>().material;
        material.SetColor("_BaseColor1", leaderColor1);
        material.SetColor("_BaseColor2", leaderColor2);
        swarmList = new List<Boid>();
    }

    public int getCellInfo()
    {
        return cellGroups.GetIndex(transform.position);
    }


    public void AddBoidToSwarm(Boid boid)
    {
        swarmList.Add(boid);
    }


    public void RemoveBoidFromSwarm(Boid boid)
    {
        swarmList.Remove(boid);
    }


    public List<Boid> GetSwarmList()
    {
        return swarmList;
    }

    public Vector3 getPosition()
    {
        return transform.position;
    }


}
