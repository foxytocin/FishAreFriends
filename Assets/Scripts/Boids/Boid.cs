using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Boid : MonoBehaviour
{

    Predator predator;
    Leader leader;
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
        leader = FindObjectOfType<Leader>();
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
        delay = Random.Range(5, 31);
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

        cellIndex = cellGroups.GetIndex(transform.position);
        cellGroups.RegisterAtCell(this);
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

    public void UpdateBoid()
    {
        cellGroups.CheckCell(this);

        // if (
        //     position.x < -1 || position.x > mapGenerator.mapSize.x + 1 ||
        //     position.y < -1 || position.y > mapGenerator.mapSize.y + 1 ||
        //     position.z < -1 || position.z > mapGenerator.mapSize.z + 1
        // )
        // {
        //     LetMeDie();
        // }

        Vector3 acceleration = Vector3.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }


        Collider[] hitCollidersPredator = new Collider[1];
        if (optimiert)
        {
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
        }
        else
        {
            // OHNE OPTIMIERUNG
            hitCollidersPredator = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Predator"));
            if (hitCollidersPredator.Length > 0)
            {
                GameObject predator = hitCollidersPredator[0].gameObject;
                var positionToPredator = predator.transform.position - position;
                acceleration += positionToPredator * -(settings.predatorAvoidanceForce);

                Predator predatorScript = predator.GetComponent<Predator>();
                predatorScript.IAmYourBoid(gameObject);

                float distanceToPredator = Vector3.Distance(position, predator.transform.position);
                if (distanceToPredator <= 1.5f)
                {
                    if (predatorScript.BoidDied(this, foodLeft))
                    {
                        Instantiate(prefabBlood, position, Quaternion.identity);
                        ecoSystemManager.addKilledFish();
                        LetMeDie();
                    }

                }
            }
        }


        if (optimiert)
        {
            // if (cellIndex == leader.getCellInfo())
            // {
            float distanceToLeader = Vector3.Distance(position, leader.getPosition());
            if (distanceToLeader < 2f) //Predator-Test fehlt
            {
                Vector3 positionToLeader = leader.getPosition() - position;
                acceleration += positionToLeader * settings.leadingForce;

                setColor(leader.leaderColor1, leader.leaderColor2);

                if (myLeader == null)
                {
                    myLeader = leader;
                    leader.AddBoidToSwarm(this);
                }
            }
            else
            {
                setColor(originalColor1, originalColor2);

                if (myLeader != null)
                {
                    myLeader.RemoveBoidFromSwarm(this);
                    myLeader = null;
                }

            }
            // }


        }
        else
        {

            //follow leader
            Collider[] hitCollidersLeader = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Leader"));
            if (hitCollidersPredator.Length == 0 && hitCollidersLeader.Length > 0)
            {
                GameObject leaderGameObject = hitCollidersLeader[0].gameObject;
                var positionToLeader = leaderGameObject.transform.position - position;
                acceleration += positionToLeader * settings.leadingForce;


                Leader leaderScript = hitCollidersLeader[0].gameObject.GetComponent<Leader>();
                if (leaderScript == null)
                    return;

                setColor(leaderScript.leaderColor1, leaderScript.leaderColor2);

                if (myLeader == null)
                {
                    myLeader = leaderScript;
                    leaderScript.AddBoidToSwarm(this);
                }

            }
            else
            {
                setColor(originalColor1, originalColor2);

                if (myLeader != null)
                {
                    myLeader.RemoveBoidFromSwarm(this);
                    myLeader = null;
                }

            }
        }


        if (optimiert)
        {
            setFoodNeeds();
            if ((foodLeft < 400 && myLeader == null) || foodLeft < 200)
            {
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
                        var positionToFood = foodList[nearestFoodIndex].GetPosition() - position;
                        acceleration += positionToFood * settings.chaisingForFoodForce;

                        //Debug.DrawRay(position, positionToFood, Color.red);

                        // eat when nearby
                        if (Vector3.Distance(transform.position, foodList[nearestFoodIndex].GetPosition()) <= (foodList[nearestFoodIndex].transform.localScale.x / 2f) + 0.5f)
                        {
                            setColor(originalColor1, originalColor2);

                            foodNeeds -= foodList[nearestFoodIndex].getFood(foodNeeds);
                            setFoodNeeds();
                        }
                    }
                }
            }


        }
        else
        {

            // chaising for food
            setFoodNeeds();
            if ((foodLeft < 400 && myLeader == null) || foodLeft < 200)
            {
                setColor(Color.red, Color.red);

                Collider[] hitCollidersFood = Physics.OverlapSphere(transform.position, 10, LayerMask.GetMask("Food"));
                if (hitCollidersFood.Length > 0)
                {
                    // find nearest food
                    float nearestFood = float.PositiveInfinity;
                    int nearestFoodIndex = 0;
                    for (int i = 0; i < hitCollidersFood.Length; i++)
                    {
                        float dist = Vector3.Distance(transform.position, hitCollidersFood[i].gameObject.transform.position);
                        if (dist < nearestFood)
                        {
                            nearestFood = dist;
                            nearestFoodIndex = i;
                        }
                    }

                    // get nearest food gameobject
                    GameObject fo = hitCollidersFood[nearestFoodIndex].gameObject;

                    if (fo)
                    {
                        // swim towards food
                        var positionToFood = fo.transform.position - position;
                        acceleration += positionToFood * settings.chaisingForFoodForce;

                        //Debug.DrawRay(position, positionToFood, Color.red);

                        // eat when nearby
                        if (Vector3.Distance(transform.position, fo.transform.position) <= (fo.transform.localScale.x / 2f) + 0.5f)
                        {
                            setColor(originalColor1, originalColor2);

                            foodNeeds -= fo.GetComponent<Food>().getFood(foodNeeds);
                            setFoodNeeds();
                        }
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
            StopCoroutine(IncreaseFood());
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
        gameObject.transform.forward = Random.insideUnitSphere;

        // reset state
        ecoSystemManager.addFishToFishCount();
        //gameObject.SetActive(true);
        alife = true;
        setColor(originalColor1, originalColor2);
        setWobbleSpeed(originalWobbleSpeed);

        StartCoroutine(IncreaseFood());
    }

    bool IsHeadingForCollision()
    {
        if (showDebug)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
            Debug.DrawRay(position, forward, Color.green);
        }

        // if (position.x < 5 || position.x > mapGenerator.mapSize.x - 5 ||
        // position.y < 5 || position.y > mapGenerator.mapSize.y - 5 ||
        // position.z < 5 || position.z > mapGenerator.mapSize.z - 5)
        // {
        RaycastHit hit;
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask))
        {
            return true;
        }
        else { }
        return false;
        // }
        // return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;


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