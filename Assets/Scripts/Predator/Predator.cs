using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    Transform cachedTransform;

    public float minSpeed = 2;
    public float maxSpeed = 4.5f;
    public float maxSteerForce = 2;
    public float avoidCollisionWeight = 5;
    public float boundsRadius = 0.27f;
    public float collisionAvoidDst = 5;
    public LayerMask obstacleMask;

    // hunting
    GameObject boidToHunt = null;
    public int fishNutritionalValue = 700;

    // food
    public int basicFoodNeed = 1000;
    public int foodNeeds;

    // Debug
    bool showDebug = true;


    void Awake()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Obstacle");
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        cachedTransform = transform;
    }

    public void Start()
    {
        foodNeeds = basicFoodNeed;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = minSpeed;
        velocity = transform.forward * startSpeed;
    }

    public void IAmYourBoid(GameObject boid)
    {
        if (boidToHunt == null)
            boidToHunt = boid;
    }

    public bool BoidDied(Boid boid)
    {

        if (!boidToHunt.Equals(boid.gameObject))
            return false;

        boidToHunt = null;

        if (foodNeeds >= basicFoodNeed)
            return false;

        foodNeeds += fishNutritionalValue;
        return true;
    }


    // Update is called once per frame
    void LateUpdate()
    {

        // foodNeeds
        foodNeeds -= 1;

        ecoSystemManager.setFoodDemandPredator(foodNeeds);

        Vector3 acceleration = Vector3.zero;
        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        if (boidToHunt != null && foodNeeds < 500)
        {
            var positionToBoid = boidToHunt.transform.position - position;
            acceleration += positionToBoid * 5;

            float distance = Vector3.Distance(position, boidToHunt.transform.position);

            if (!boidToHunt.activeSelf)
                boidToHunt = null;

            if (distance > 10)
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
}
