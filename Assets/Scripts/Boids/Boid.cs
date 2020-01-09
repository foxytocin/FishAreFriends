using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class Boid : MonoBehaviour
{
    Predator predator;
    Leader otherLeader;
    CellGroups cellGroups;
    BoidSettings settings;
    public GameObject prefabBlood;
    EcoSystemManager ecoSystemManager;
    MapGenerator mapGenerator;
    FoodManager foodManager;

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

    public bool optimiert;

    // Cached
    Material[] material;

    Transform cachedTransform;
    public Transform target;

    // Swarm handling varialbes
    Leader myLeader = null;
    public int cellIndex = 0;

    // Food
    public int basicFoodNeed = 1000;
    public int foodNeeds = 0;
    public int foodLeft;
    public int hungerRate = 1;
    public bool alife = true;
    private int delay = 30;
    private bool firstTime = true;

    public int timeBetweedFoodUpdate = 3;

    public Status status;
    public enum Status {
        swimmsToFood,
        normalSwimming,
        died
    }

    Coroutine updateBoidNormalMovement = null;
    Coroutine calculateFoodBehavior = null;
    List<Food> foodList;

    // Debug
    bool showDebug = false;


    void Awake()
    {
        alife = true;
        foodManager = FindObjectOfType<FoodManager>();
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        cellGroups = FindObjectOfType<CellGroups>();
        predator = FindObjectOfType<Predator>();
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
        foodList = foodManager.GetFoodList();

        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;

        setColor(originalColor1, originalColor2);

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

            

            if(status == Status.normalSwimming) {
                yield return new WaitForSeconds(delay);
            }
             
            if(status == Status.swimmsToFood) {
                yield return new WaitForSeconds(0.5f);
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foodNeeds += hungerRate;


            if (firstTime)
            {
                firstTime = false;
                delay = timeBetweedFoodUpdate;
            }


            setFoodNeeds();
            // boid already has a food-target: swimm towards the target
            if(foodTarget != null && foodTarget.checkAmount() > 0) {

                //Debug.Log("Schwimmt zur bekannten Futterquelle");
                accelerationFoodBehavior = (foodPosition - position) * settings.chaisingForFoodForce;

            } else {

                // find food 
                foodTarget = null;
                status = Status.normalSwimming;
                

                if ((foodLeft < 400 && myLeader == null) || foodLeft < 200)
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
            if(status == Status.swimmsToFood) {

                if (Vector3.Distance(transform.position, foodPosition) <= (foodTarget.transform.localScale.x / 2f) + 0.5f)
                {
                    setColor(originalColor1, originalColor2);

                    foodNeeds -= foodTarget.getFood(foodNeeds);
                    setFoodNeeds();

                    accelerationFoodBehavior = Vector3.zero;
                    foodTarget = null;
                    status = Status.normalSwimming;

                    //Debug.Log("Hat gefressen");
                }
            }
            
            sw.Stop();
            Debug.Log("FoodNeeds Secons to Execude: " +sw.ElapsedMilliseconds);

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

        while(true) {

            accelerationBehaviorChanges = Vector3.zero;

            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidDir = ObstacleRays();
                Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
                accelerationBehaviorChanges += collisionAvoidForce;
            }


            // avoid predator
            float distanceToPredator = Vector3.Distance(position, predator.getPosition());
            if (distanceToPredator < 5f)
            {
                Vector3 positionToPredator = predator.getPosition() - position;
                accelerationBehaviorChanges += positionToPredator * -(settings.predatorAvoidanceForce);

                predator.IAmYourBoid(gameObject);

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
                Vector3 positionToLeader = otherLeader.getPosition() - position;
                accelerationBehaviorChanges += positionToLeader * settings.leadingForce;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }



    public void JoinNewSwarm(Leader leader)
    {
        myLeader = leader;
        leader.AddBoidToSwarm(this);
        setColor(leader.leaderColor1, leader.leaderColor2);
    }

    public void LeaveActualSwarm()
    {
        setColor(originalColor1, originalColor2);
        myLeader.RemoveBoidFromSwarm(this);
        myLeader = null;
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
        setColor(originalColor1, originalColor2);
        setWobbleSpeed(originalWobbleSpeed);

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









/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class Boid : MonoBehaviour
{

    Predator predator;
    Leader otherLeader;
    CellGroups cellGroups;
    BoidSettings settings;
    public GameObject prefabBlood;
    EcoSystemManager ecoSystemManager;
    MapGenerator mapGenerator;
    FoodManager foodManager;

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
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    public Vector3 dir;

    public bool optimiert;

    // Cached
    Material[] material;

    Transform cachedTransform;
    public Transform target;

    // Swarm handling varialbes
    Leader myLeader = null;
    public int cellIndex = 0;

    // Food
    public int basicFoodNeed = 1000;
    public int foodNeeds = 0;
    public int foodLeft;
    public int hungerRate = 1;
    public bool alife = true;
    private int delay = 30;
    private bool firstTime = true;

    public int intervallToCheckFood = 3;
    private float checkForFood;

    Coroutine increaseFood = null;
    public Status status;
    public enum Status {
        swimmsToFood,
        normalSwimming,
        died
    }


    // Debug
    bool showDebug = false;

    void Awake()
    {
        alife = true;
        foodManager = FindObjectOfType<FoodManager>();
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        cellGroups = FindObjectOfType<CellGroups>();
        predator = FindObjectOfType<Predator>();
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
        status = Status.normalSwimming;
        checkForFood = (float)intervallToCheckFood;
    }


    public void Initialize(BoidSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;

        setColor(originalColor1, originalColor2);
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


    private IEnumerator IncreaseFood()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            foodNeeds += hungerRate;

            if (firstTime)
            {
                firstTime = false;
                delay = 1;
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
        Vector3 acceleration = acceleration_;

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }


        // avoid predator
        float distanceToPredator = Vector3.Distance(position, predator.getPosition());
        if (distanceToPredator < 5f)
        {
            Vector3 positionToPredator = predator.getPosition() - position;
            acceleration += positionToPredator * -(settings.predatorAvoidanceForce);

            predator.IAmYourBoid(gameObject);

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
            Vector3 positionToLeader = otherLeader.getPosition() - position;
            acceleration += positionToLeader * settings.leadingForce;
        }



        // find food
        if(status == Status.swimmsToFood)
            checkForFood = 0.25f;

        if(status == Status.normalSwimming)
            checkForFood -= Time.deltaTime;

        setFoodNeeds();
        if (checkForFood < 0 && (foodLeft < 400 && myLeader == null) || foodLeft < 200)
        {
            checkForFood = intervallToCheckFood;
            setColor(Color.red, Color.red);

            List<Food> foodList = foodManager.GetFoodList();
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

                if (nearestFood < 15f)
                {
                    // swim towards food
                    status = Status.swimmsToFood;

                    var positionToFood = foodList[nearestFoodIndex].GetPosition() - position;
                    acceleration += positionToFood * settings.chaisingForFoodForce;

                    // eat when nearby
                    if (Vector3.Distance(transform.position, foodList[nearestFoodIndex].GetPosition()) <= (foodList[nearestFoodIndex].transform.localScale.x / 2f) + 0.5f)
                    {
                        setColor(originalColor1, originalColor2);

                        foodNeeds -= foodList[nearestFoodIndex].getFood(foodNeeds);
                        setFoodNeeds();

                        status = Status.normalSwimming;
                    }
                }
            }
        }


        velocity += acceleration * Time.deltaTime;
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


    public void JoinNewSwarm(Leader leader)
    {
        myLeader = leader;
        leader.AddBoidToSwarm(this);
        setColor(leader.leaderColor1, leader.leaderColor2);
    }

    public void LeaveActualSwarm()
    {
        setColor(originalColor1, originalColor2);
        myLeader.RemoveBoidFromSwarm(this);
        myLeader = null;
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
        }
    }


    public void LetMeDie()
    {
        if (alife)
        {
            StopCoroutine(increaseFood);
            alife = false;
            ecoSystemManager.addDiedFish();
            StartDeadAnimation();
        }
    }


    public void StartDeadAnimation()
    {
        StartCoroutine(Animate());
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
        //gameObject.SetActive(true);
        alife = true;
        status = Status.normalSwimming;
        setColor(originalColor1, originalColor2);
        setWobbleSpeed(originalWobbleSpeed);

        increaseFood = StartCoroutine(IncreaseFood());
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
*/