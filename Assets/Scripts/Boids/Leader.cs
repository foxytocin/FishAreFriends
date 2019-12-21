using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour
{

    private List<Boid> swarmList;

    public Color leaderColor1;
    public Color leaderColor2;
    private Material material;

    private void Awake()
    {
        //leaderColor = Random.ColorHSV();
        material = gameObject.GetComponentInChildren<MeshRenderer>().material;
        material.SetColor("_BaseColor1", leaderColor1);
        material.SetColor("_BaseColor2", leaderColor2);
        swarmList = new List<Boid>();
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


}
