using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour
{
    
    private List<Boid> swarmList;
    public Color leaderColor;

    private void Awake()
    {
        leaderColor = Random.ColorHSV();
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
