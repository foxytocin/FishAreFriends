using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class Boid : MonoBehaviour
{
    List<Predator> availablePredators;
    CellGroups cellGroups;
    BoidSettings settings;
    public GameObject prefabBlood;
    EcoSystemManager ecoSystemManager;
    MapGenerator mapGenerator;

    private Color originalColor1;
    private Color originalColor2;
    private float originalWobbleSpeed;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    public Vector3 velocity;

    // To update:
    Vector3 accelerationNormalMoving = Vector3.zero;
    Vector3 accelerationBehaviorChanges = Vector3.zero;
    Vector3 accelerationFoodBehavior = Vector3.zero;


    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    public Vector3 dir;


    // Cached
    Material[] material;

    Transform cachedTransform;
    public Transform target;

    // Swarm handling varialbes
    private Leader myLeader = null;
    public int cellIndex = 0;

    // Food
    public static int basicFoodNeed = 1000;
    public static int thresholdHungry = 400;
    public static int thresholdStarving = 200;
    public int foodNeeds = 0;
    public int foodLeft;
    public int hungerRate = 1;
    public bool alife = true;
    private int delay = 30;
    private bool firstTime = true;

    public int timeBetweedFoodUpdate = 3;

    public Status status;
    public enum Status
    {
        swimmsToFood,
        normalSwimming,
        died
    }

    public bool hungry = false;
    public bool starving = false;

    Coroutine updateBoidNormalMovement = null;
    Coroutine calculateFoodBehavior = null;
    List<Food> foodList;

    // Debug
    bool showDebug = false;


    void Awake()
    {
        alife = true;
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        cellGroups = FindObjectOfType<CellGroups>();
        // leader = FindObjectOfType<Leader>();
        foodNeeds = 0;
        foodLeft = basicFoodNeed;
        material = new Material[4];
        material[0] = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        material[1] = gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material;
        material[2] = gameObject.transform.GetChild(2).GetComponent<MeshRenderer>().material;
        material[3] = gameObject.transform.GetChild(3).GetComponent<MeshRenderer>().material;
        originalWobbleSpeed = 1;
        cachedTransform = transform;
        position = cachedTransform.position;
        delay = UnityEngine.Random.Range(5, 31);
        hungerRate = hungerRate * timeBetweedFoodUpdate;
        status = Status.normalSwimming;
    }

    public void Initialize(BoidSettings settings, Transform target)
    {
        foodList = FoodManager.foodList;
        availablePredators = Predator.availablePredators;

        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;

        setColor(originalColor1, originalColor2);

        cellGroups.RegisterAtCell(this);
        cellGroups.CheckCell(this);

        updateBoidNormalMovement = StartCoroutine(UpdateBoidNormalMovement());
    }


    public void PassColor(Color col1, Color col2)
    {
        if (material != null)
        {
            originalColor1 = col1;
            originalColor2 = col2;
            setColor(originalColor1, originalColor2);
        }
    }


    Food foodTarget = null;
    Vector3 foodPosition;

    private IEnumerator CalculateFoodBehavior()
    {
        while (true)
        {
            if (status != Status.swimmsToFood)
            {
                yield return new WaitForSeconds(delay);
            }

            if (status == Status.swimmsToFood)
            {
                yield return new WaitForSeconds(0.5f);
            }

            foodNeeds += hungerRate;


            if (firstTime)
            {
                firstTime = false;
                delay = timeBetweedFoodUpdate;
            }


            setFoodNeeds();
            // boid already has a food-target: swimm towards the target
            if (foodTarget != null && foodTarget.checkAmount() > 0)
            {
                //Debug.Log("Schwimmt zur bekannten Futterquelle");
                accelerationFoodBehavior = (foodPosition - position) * settings.chaisingForFoodForce;
            }
            else
            {
                // find food 
                foodTarget = null;
                status = Status.normalSwimming;

                if ((hungry && myLeader == null) || starving)
                {
                    //Debug.Log("Keine Futterquelle vorhanden: suche");
                    setColor(Color.red, Color.red);

                    if (foodList.Count > 0)
                    {
                        // find nearest food
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

                        // found food: setting food-parameters
                        if (nearestFood < 15f)
                        {
                            //Debug.Log("Futterquelle gefunden");
                            // swim towards food
                            status = Status.swimmsToFood;

                            foodTarget = foodList[nearestFoodIndex];
                            foodPosition = foodTarget.GetPosition();
                            accelerationFoodBehavior = (foodPosition - position) * settings.chaisingForFoodForce;
                        }
                    }
                }
            }

            // tests if reached the food-source: eat when nearby
            if (status == Status.swimmsToFood)
            {

                if (Vector3.Distance(transform.position, foodPosition) <= (foodTarget.transform.localScale.x / 2f) + 0.5f)
                {
                    foodNeeds -= foodTarget.getFood(foodNeeds);
                    setFoodNeeds();

                    accelerationFoodBehavior = Vector3.zero;
                    foodTarget = null;
                    status = Status.normalSwimming;

                    //Debug.Log("Hat gefressen");
                }
            }
            else
            {

                accelerationFoodBehavior = Vector3.zero;
            }
        }
    }


    public bool CollisionAhead()
    {
        return (
            position.x < 5 || position.x > mapGenerator.mapSize.x - 5 ||
            position.y < 5 || position.y > mapGenerator.mapSize.y - 5 ||
            position.z < 5 || position.z > mapGenerator.mapSize.z - 5
        );
    }


    public void UpdateBoid(Vector3 acceleration_)
    {
        accelerationNormalMoving = accelerationBehaviorChanges + accelerationFoodBehavior + acceleration_;

        velocity += accelerationNormalMoving * Time.deltaTime;
        float speed = velocity.magnitude;

        dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;

        float ws = material[0].GetFloat("_WobbleSpeed");
        if (ws != speed && speed > 0)
        {
            ws = Mathf.Lerp(ws, speed, 0.1f * Time.deltaTime);
        }

        setWobbleSpeed(Mathf.Clamp(ws, 0.2f, 10f));
    }


    public IEnumerator UpdateBoidNormalMovement()
    {

        while (true)
        {

            accelerationBehaviorChanges = Vector3.zero;

            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidDir = ObstacleRays();
                Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
                accelerationBehaviorChanges += collisionAvoidForce;
            }


            // avoid predator
            foreach (Predator predator in availablePredators)
            {
                float distanceToPredator = Vector3.Distance(position, predator.getPosition());
                if (distanceToPredator < 5f)
                {
                    Vector3 positionToPredator = predator.getPosition() - position;
                    accelerationBehaviorChanges += positionToPredator * -(settings.predatorAvoidanceForce);

                    predator.IAmYourBoid(this);

                    if (distanceToPredator <= 1.5f)
                    {
                        if (predator.BoidDied(this, foodLeft))
                        {
                            Instantiate(prefabBlood, position, Quaternion.identity);
                            ecoSystemManager.addKilledFish();
                            LetMeDie();
                        }
                    }
                }
            }


            Leader otherLeader = null;

            // find leader
            float distanceToLeader = float.MaxValue;
            if (Leader.leaderList != null)
            {
                foreach (Leader l in Leader.leaderList)
                {
                    float tempDistance = Vector3.Distance(position, l.getPosition());
                    if (tempDistance < distanceToLeader)
                    {
                        otherLeader = l;
                        distanceToLeader = tempDistance;
                    }
                }
            }


            // if i have no leader already, use the nearest, if distance < 5
            if (myLeader == null && distanceToLeader <= 5f)
                JoinNewSwarm(otherLeader);

            // if i have a leader but he is to far away, reset follow leader
            if (myLeader != null && distanceToLeader > 5f)
                LeaveActualSwarm();

            // if i have a leader
            if (myLeader != null)
            {
                Vector3 positionToLeader = myLeader.getPosition() - position;
                accelerationBehaviorChanges += positionToLeader * settings.leadingForce;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }



    private void JoinNewSwarm(Leader leader)
    {
        myLeader = leader;
        leader.AddBoidToSwarm(this);
        setColor(leader.leaderColor1, leader.leaderColor2);
    }

    private void LeaveActualSwarm()
    {
        setColor(originalColor1, originalColor2);
        myLeader.RemoveBoidFromSwarm(this);
        myLeader = null;
    }

    public void ToggleActualSwarm(Leader newLeader)
    {
        newLeader.AddBoidToSwarm(this);
        myLeader.RemoveBoidFromSwarm(this);

        myLeader = newLeader;
        setColor(newLeader.leaderColor1, newLeader.leaderColor2);
    }


    public void setColor(Color col1, Color col2)
    {
        for (int i = 0; i < material.Length; i++)
        {
            material[i].SetColor("_BaseColor1", col1);
            material[i].SetColor("_BaseColor2", col2);
        }
    }

    public void setWobbleSpeed(float speed)
    {
        for (int i = 0; i < 2; i++)
        {
            material[i].SetFloat("_WobbleSpeed", speed);
        }
    }


    void setFoodNeeds()
    {
        if (foodNeeds > basicFoodNeed)
        {
            LetMeDie();
        }
        else
        {
            foodNeeds = Mathf.Clamp(foodNeeds, 0, basicFoodNeed);
            foodLeft = basicFoodNeed - foodNeeds;

            starving = (foodLeft < thresholdStarving) ? true : false;
            hungry = (foodLeft < thresholdHungry) ? true : false;

            if (!starving)
            {
                if (myLeader == null)
                    setColor(originalColor1, originalColor2);
                else
                    setColor(myLeader.leaderColor1, myLeader.leaderColor2);

                status = Status.normalSwimming;
                foodTarget = null;
            }
        }
    }


    public void LetMeDie()
    {
        if (alife)
        {
            StopCoroutine(calculateFoodBehavior);
            StopCoroutine(updateBoidNormalMovement);

            alife = false;
            ecoSystemManager.addDiedFish();

            StartCoroutine(Animate());
        }
    }


    private IEnumerator Animate()
    {
        while (transform.position.y < mapGenerator.mapSize.y * 1.3f)
        {
            transform.position += new Vector3(0, 1f * Time.deltaTime, 0);
            yield return new WaitForEndOfFrame();
        }

        RespawnBoid();
    }


    public void RespawnBoid()
    {
        // reset foold
        foodNeeds = 0;
        foodLeft = basicFoodNeed;

        // reset position
        gameObject.transform.position = ecoSystemManager.GetNextSpawnPoint();
        gameObject.transform.forward = UnityEngine.Random.insideUnitSphere;

        // reset state
        ecoSystemManager.addFishToFishCount();


        alife = true;
        status = Status.normalSwimming;
        foodTarget = null;

        setColor(originalColor1, originalColor2);
        setWobbleSpeed(originalWobbleSpeed);

        cellGroups.CheckCell(this);

        calculateFoodBehavior = StartCoroutine(CalculateFoodBehavior());
        updateBoidNormalMovement = StartCoroutine(UpdateBoidNormalMovement());
    }

    bool IsHeadingForCollision()
    {
        if (showDebug)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
            Debug.DrawRay(position, forward, Color.green);
        }

        RaycastHit hit;
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask))
        {
            return true;
        }
        else { }
        return false;
    }


    // use this to check if oppenent want to hunt a boid
    //  check boid is allready in a swarm
    public bool HasLeader()
    {
        return myLeader != null;
    }


    Vector3 ObstacleRays()
    {
        float3[] rayDirections = BoidHelper.directions;


        for (int i = 0; i < rayDirections.Length; i++)
        {

            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);

            if (showDebug)
            {
                Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
                Debug.DrawRay(position, forward, Color.green);
            }

            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }


    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

}