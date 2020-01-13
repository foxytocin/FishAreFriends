using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour
{

    ForceField forceField;
    CellGroups cellGroups;
    MapGenerator mapGenerator;
    GuiOverlay guiOverlay;
    private List<Boid> swarmList;

    public static List<Leader> leaderList;

    public Color leaderColor1;
    public Color leaderColor2;
    private Material material;

    // defined as secounds
    private float waitForNextRipCount;
    private bool humanPlayer = false;

    private void Awake()
    {   
        swarmList = new List<Boid>();
        material = gameObject.GetComponentInChildren<MeshRenderer>().material;
        forceField = GetComponentInChildren<ForceField>();
        cellGroups = FindObjectOfType<CellGroups>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        guiOverlay = FindObjectOfType<GuiOverlay>();
        transform.position = mapGenerator.mapSize / 2;

        if (leaderList == null)
            leaderList = new List<Leader>();

        leaderList.Add(this);

        // defined as secounds
        waitForNextRipCount = 0;

        
    }

    private void Start() {
        material.SetColor("_BaseColor1", leaderColor1);
        material.SetColor("_BaseColor2", leaderColor2);
        forceField.SetColor(leaderColor1);

        if(gameObject.name == "Leader")
            humanPlayer = true;
    }


    public int getCellInfo()
    {
        return cellGroups.GetIndex(transform.position);
    }


    public void AddBoidToSwarm(Boid boid)
    {
        swarmList.Add(boid);

        if(humanPlayer)
            guiOverlay.SetPlayerSwarmSize(swarmList.Count);
    }


    public void RemoveBoidFromSwarm(Boid boid)
    {
        swarmList.Remove(boid);

        if(humanPlayer)
            guiOverlay.SetPlayerSwarmSize(swarmList.Count);
    }

    public int GetSwarmSize()
    {
        return swarmList.Count;
    }

    public List<Boid> GetSwarmList()
    {
        return swarmList;
    }

    public Vector3 getPosition()
    {
        return transform.position;
    }


    public int GetSwarmEnergie() {

        int energie = 0;
        foreach(Boid boid in swarmList) {
            energie += boid.foodLeft;
        }

        int swarmSize = GetSwarmSize();

        return (swarmSize > 0) ? (energie / swarmSize) : 0;
    }


    public void LateUpdate()
    {

        if(waitForNextRipCount > 0)
        {
            waitForNextRipCount -= Time.deltaTime;
            return;
        }


        Leader otherLeader = null;
        // find other leader
        float distanceToOtherLeader = float.MaxValue;
        if (leaderList != null)
        {
            foreach (Leader l in Leader.leaderList)
            {
                if (l.Equals(this))
                    continue;

                float tempDistance = Vector3.Distance(transform.position, l.getPosition());
                if (tempDistance < distanceToOtherLeader)
                {
                    otherLeader = l;
                    distanceToOtherLeader = tempDistance;
                }
            }
        }


        if(distanceToOtherLeader <= 5f && otherLeader != null)
        {
            Debug.Log("Found other leader");
            int otherSwarmCount = otherLeader.GetSwarmSize();
            int mySwarmCount = GetSwarmSize();

            forceField.StartPulse();

            // if my swarm is extremly bigger than the other swarm
            //  i get half the boids of the other
            int ripCount = 0;

            if (otherSwarmCount < mySwarmCount)
                ripCount = otherSwarmCount / 4;

            if (otherSwarmCount * 2 < mySwarmCount)
                ripCount = otherSwarmCount / 3;

            if (otherSwarmCount * 3 < mySwarmCount)
                ripCount = otherSwarmCount / 2;

            if(ripCount != 0)
            {
                Debug.Log("Swarm riped " + ripCount + " boids from " + otherSwarmCount );
                List<Boid> boidsToRip = otherLeader.GetSwarmList().GetRange(0, otherSwarmCount < ripCount ? otherSwarmCount : ripCount);

                // lets rip
                foreach (Boid boid in boidsToRip)
                {
                    boid.LeaveActualSwarm();
                    boid.JoinNewSwarm(this);
                }
            }

            waitForNextRipCount = 10f;

        } else {
            
            forceField.StopPulse();
        }

        guiOverlay.SetPlayerEnergie(GetSwarmEnergie());
    }


}
