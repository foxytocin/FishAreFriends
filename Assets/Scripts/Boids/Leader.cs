using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Leader : MonoBehaviour
{

    ForceField forceField;
    EnergyDisplay energyDisplay;
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
    private int availableEnergie = 0;
    private int hungerOfSwarm = 0;
    private List<Food> foodList;
    private bool coroutineFeedSwarmRunning = false;

    // food
    private bool boidAddedWhileCheckFoodNeeds = false;

    private void Awake()
    {
        swarmList = new List<Boid>();
        energyDisplay = FindObjectOfType<EnergyDisplay>();
        material = gameObject.GetComponentInChildren<MeshRenderer>().material;
        forceField = GetComponentInChildren<ForceField>();
        cellGroups = FindObjectOfType<CellGroups>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        guiOverlay = FindObjectOfType<GuiOverlay>();
        //transform.position = mapGenerator.mapSize / 2;

        if (leaderList == null)
            leaderList = new List<Leader>();

        leaderList.Add(this);

        // defined as secounds
        waitForNextRipCount = 0;

        foodList = FoodManager.foodList;
    }


    private void Start()
    {
        if (leaderList != null) {
            leaderList = new List<Leader>();
            leaderList.Add(this);
        }
            
        material.SetColor("_BaseColor1", leaderColor1);
        material.SetColor("_BaseColor2", leaderColor2);
        forceField.SetColor(leaderColor1);

        if (gameObject.name == "Leader")
            humanPlayer = true;
    }


    public int getCellInfo()
    {
        return cellGroups.GetIndex(transform.position);
    }


    public void AddBoidToSwarm(Boid boid)
    {
        swarmList.Add(boid);

        if (humanPlayer)
            guiOverlay.SetPlayerSwarmSize(swarmList.Count);

        if (!coroutineFeedSwarmRunning)
            StartCoroutine(FeedSwarm());
        else
            boidAddedWhileCheckFoodNeeds = true;
    }


    public void RemoveBoidFromSwarm(Boid boid)
    {
        swarmList.Remove(boid);

        if (humanPlayer)
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


    public void CalculateSwarmFoodNeeds()
    {
        int energie = 0;
        int boidsHungry = 0;
        int boidsStarving = 0;
        hungerOfSwarm = 0;

        foreach (Boid boid in swarmList)
        {
            energie += boid.foodLeft;
            hungerOfSwarm += boid.foodNeeds;

            boidsHungry += (boid.hungry) ? 1 : 0;
            boidsStarving += (boid.starving) ? 1 : 0;
        }

        int swarmSize = GetSwarmSize();

        if (humanPlayer)
        {
            //guiOverlay.SetDebugInfo("FoodNeed: " + hungerOfSwarm + " | H: " + boidsHungry + " | S: " + boidsStarving + " | FoodAv: " + availableEnergie);

            int avgEnergieSwarm = (swarmSize > 0) ? (energie / swarmSize) : 0;
            //guiOverlay.SetPlayerEnergie(avgEnergieSwarm);
            energyDisplay.SetEnergyStatus(avgEnergieSwarm);
        }
    }


    public void LateUpdate()
    {
        if(guiOverlay.gameStatus == GuiOverlay.GameStatus.inGame) {
            if (humanPlayer)
            {
                CalculateSwarmFoodNeeds();
                CheckFoodSource();
            }


            if (waitForNextRipCount > 0)
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


            if (distanceToOtherLeader <= 5f && otherLeader != null)
            {
                // Debug.Log("Found other leader");
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

                if (ripCount != 0)
                {
                    Debug.Log("Swarm riped " + ripCount + " boids from " + otherSwarmCount);
                    List<Boid> boidsToRip = otherLeader.GetSwarmList().GetRange(0, otherSwarmCount < ripCount ? otherSwarmCount : ripCount);

                    // lets rip
                    foreach (Boid boid in boidsToRip)
                    {
                        boid.ToggleActualSwarm(this);
                    }
                }

                waitForNextRipCount = 10f;
            }
            else
            {
                forceField.StopPulse();
            }
        }
    }


    private void CheckFoodSource()
    {
        if (foodList.Count > 0)
        {
            float nearestFood = float.PositiveInfinity;
            int nearestFoodIndex = 0;
            for (int i = 0; i < foodList.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, foodList[i].GetPosition());
                if (dist < nearestFood)
                {
                    nearestFood = dist;
                    nearestFoodIndex = i;
                }
            }

            Food foodTarget = foodList[nearestFoodIndex];
            Vector3 foodPosition = foodTarget.GetPosition();

            if (Vector3.Distance(transform.position, foodPosition) <= (foodTarget.transform.localScale.x / 2f) + 1f)
            {
                int neededFood = hungerOfSwarm - availableEnergie;
                neededFood = (neededFood > 0) ? neededFood : 0;
                int gotFromSource = foodTarget.getFood(neededFood);
                availableEnergie += gotFromSource;
                //Debug.Log("Hunger of Swarm: " + hungerOfSwarm + " / Got from FoodSource: " + gotFromSource);
            }
        }

        // feed the swarm if not alreay feeding
        if (!coroutineFeedSwarmRunning && availableEnergie > 0)
            StartCoroutine(FeedSwarm());
    }


    private IEnumerator FeedSwarm()
    {
        coroutineFeedSwarmRunning = true;

        do
        {
            // get existing food in boids
            int existingFoodInBoids = 0;
            foreach (Boid boid in swarmList)
                existingFoodInBoids += boid.foodLeft;

            // add collected food
            existingFoodInBoids += availableEnergie;
            availableEnergie = 0;

            // set food in boids
            int foodViaBoid = existingFoodInBoids / swarmList.Count;
            //Debug.Log(foodViaBoid);
            //Debug.Log(swarmList.Count);
            foreach (Boid boid in swarmList)
                boid.setFoodNeeds(foodViaBoid);


            boidAddedWhileCheckFoodNeeds = false;
            yield return new WaitForSeconds(0.2f);

        } while (boidAddedWhileCheckFoodNeeds);
        coroutineFeedSwarmRunning = false;
    }


    public bool LeaderIsHumanPlayer()
    {
        return humanPlayer;
    }

}