using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;

public class Predator : MonoBehaviour
{

    public static List<Predator> availablePredators = new List<Predator>();
    EcoSystemManager ecoSystemManager;
    GuiOverlay guiOverlay;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    Transform cachedTransform;

    public float minSpeed = 2f;
    public float normalMaxSpeed = 3f;
    public float huntingMaxSpeed = 4.5f;
    private float maxSpeed;
    public float maxSteerForce = 2f;
    public float avoidCollisionWeight = 5f;
    public float boundsRadius = 0.27f;
    public float collisionAvoidDst = 5f;
    public LayerMask obstacleMask;

    // hunting
    Boid boidToHunt = null;
    public int fishNutritionalValue = 700;

    // food
    public int basicFoodNeed = 1000;

    public int foodNeeds;
    private bool isHunting = false;
    Material[] material;

    // Debug
    bool showDebug = false;


    void Awake()
    {
        if (availablePredators == null)
            availablePredators = new List<Predator>();
        availablePredators.Add(this);

        maxSpeed = normalMaxSpeed;
        obstacleMask = LayerMask.GetMask("Wall", "Obstacle");
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        guiOverlay = FindObjectOfType<GuiOverlay>();
        cachedTransform = transform;
        material = new Material[3];
        Transform child = gameObject.transform.GetChild(0);
        material[0] = child.GetChild(0).GetComponent<MeshRenderer>().material;
        material[1] = child.GetChild(0).GetComponent<MeshRenderer>().material;
        material[2] = child.GetChild(0).GetComponent<MeshRenderer>().material;
    }

    void Start()
    {
        foodNeeds = basicFoodNeed;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = minSpeed;
        velocity = transform.forward * startSpeed;

        StartCoroutine(DecreaseFood());
    }

    public Vector3 getPosition()
    {
        return position;
    }


    private IEnumerator DecreaseFood()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(guiOverlay.gameStatus == GuiOverlay.GameStatus.inGame)
                foodNeeds -= UnityEngine.Random.Range(2, 20);
        }
    }

    public void IAmYourBoid(Boid boid)
    {
        if (boidToHunt == null)
            boidToHunt = boid;
    }

    public bool BoidDied(Boid boid, int vitality)
    {
        if (!boidToHunt.Equals(boid))
            return false;

        boidToHunt = null;

        if (foodNeeds >= basicFoodNeed * 2)
        {
            huntingModus(false);
            return false;
        }

        foodNeeds += vitality;
        return true;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        ecoSystemManager.setFoodDemandPredator(foodNeeds);

        Vector3 acceleration = Vector3.zero;
        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        // check leaders and hunt their boids

        if (Leader.leaderList != null)
        {
            Leader leaderToAttack = null;
            float distanceToLeader = float.MaxValue;
            foreach (Leader leader in Leader.leaderList)
            {

                float tempDistance = Vector3.Distance(position, leader.getPosition());
                if (tempDistance < distanceToLeader)
                {
                    leaderToAttack = leader;
                    distanceToLeader = tempDistance;
                }
            }

            // if the nearest leaders has boids in swarm, attack him
            if (leaderToAttack != null && distanceToLeader < 30f && leaderToAttack.GetSwarmSize() > 0)
            {

                boidToHunt = leaderToAttack.GetSwarmList()[0];
                if ((foodNeeds < 200 || isHunting) && leaderToAttack.LeaderIsHumanPlayer())
                {
                    guiOverlay.DisplayMainMessage("Achtung! Der Hai hat deinen Schwarm im Visier.", 4, GuiOverlay.MessageType.warning);
                }

            }
        }


        if (boidToHunt != null && (foodNeeds < 200 || isHunting))
        {
            if (!isHunting)
                huntingModus(true);

            var positionToBoid = boidToHunt.transform.position - position;
            acceleration += positionToBoid * 5;

            float distance = Vector3.Distance(position, boidToHunt.transform.position);

            if (!boidToHunt.gameObject.activeSelf)
                boidToHunt = null;

            if (distance > 10f)
                boidToHunt = null;

        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;

        float ws = material[0].GetFloat("_SpeedZ");
        if (ws != speed)
        {
            if (speed < 0)
                speed = 0;

            ws = Mathf.Lerp(ws, speed, 0.1f * Time.deltaTime);
        }

        setWobbleSpeed(Mathf.Clamp(ws, 0.2f, 10f));
    }

    public void setWobbleSpeed(float speed)
    {
        for (int i = 0; i < material.Length; i++)
        {
            material[i].SetFloat("_SpeedZ", speed);
        }
    }


    bool IsHeadingForCollision()
    {
        if (showDebug)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
            Debug.DrawRay(position, forward, Color.green);
        }

        RaycastHit hit;
        if (Physics.SphereCast(position, boundsRadius, forward, out hit, collisionAvoidDst, obstacleMask))
        {
            return true;
        }
        else { }
        return false;
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

            if (!Physics.SphereCast(ray, boundsRadius, collisionAvoidDst, obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }

    private void huntingModus(bool value)
    {
        isHunting = value;
        maxSpeed = (isHunting) ? huntingMaxSpeed : normalMaxSpeed;

        if (isHunting)
        {
            guiOverlay.DisplayMainMessage("Der Hai ist auf der Jagt", 3, GuiOverlay.MessageType.warning);
        }
        else
        {
            guiOverlay.DisplayMainMessage("Der Hai ist satt", 3, GuiOverlay.MessageType.info);
        }

    }
}
