using UnityEngine;

public class Boid : MonoBehaviour
{

    BoidSettings settings;
    public GameObject prefabBlood;
    EcoSystemManager ecoSystemManager;

    private Color originalColor1;
    private Color originalColor2;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

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

    // Cached
    Material[] material;
    Transform cachedTransform;
    Transform target;

    // Swarm handling varialbes
    Leader myLeader = null;

    // Food
    public int basicFoodNeed = 1000;
    public int foodNeeds;
    public int foodLeft;
    private int hungerRate;
    public bool alife;

    // Debug
    bool showDebug = false;

    void Awake()
    {
        alife = true;
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        foodNeeds = 0;
        foodLeft = basicFoodNeed;
        material = new Material[3];
        material[0] = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        material[1] = gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material;
        material[2] = gameObject.transform.GetChild(2).GetComponent<MeshRenderer>().material;

        cachedTransform = transform;
        hungerRate = Random.Range(1, 3);
    }

    public void Initialize(BoidSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour(Color col1, Color col2)
    {
        if (material != null)
        {
            originalColor1 = col1;
            originalColor2 = col2;
            setColor(originalColor1, originalColor2);
        }
    }

    public void UpdateBoid()
    {
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

        // avoid predator
        Collider[] hitCollidersPredator = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Predator"));
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
                    //ecoSystemManager.addKilledFish();
                    LetMeDie();
                }

            }


        }


        // follow leader
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

                        foodNeeds -= fo.GetComponent<FoodBehavior>().getFood(foodNeeds);
                        setFoodNeeds();
                    }
                }
            }
        }


        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    void setColor(Color col1, Color col2)
    {
        foreach (Material mat in material)
        {
            mat.SetColor("_BaseColor1", col1);
            mat.SetColor("_BaseColor2", col2);
        }
    }

    void setFoodNeeds()
    {
        foodNeeds += hungerRate;
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
            alife = false;
            gameObject.SetActive(false);
        }
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
        gameObject.SetActive(true);
        alife = true;
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