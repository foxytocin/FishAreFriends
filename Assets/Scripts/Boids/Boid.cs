using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    BoidSettings settings;

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
    Material material;
    Transform cachedTransform;
    Transform target;

    // Swarm handling varialbes
    Leader myLeader = null;

    // Debug
    bool showDebug = false;

    void Awake()
    {
        material = gameObject.GetComponent<MeshRenderer>().material;
        cachedTransform = transform;
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
            material.SetColor("_BaseColor1", originalColor1);
            material.SetColor("_BaseColor2", originalColor2);
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

            material.SetColor("_BaseColor1", leaderScript.leaderColor1);
            material.SetColor("_BaseColor2", leaderScript.leaderColor2);

            if (myLeader == null)
            {
                myLeader = leaderScript;
                leaderScript.AddBoidToSwarm(this);
            }

        }
        else
        {
            material.SetColor("_BaseColor1", originalColor1);
            material.SetColor("_BaseColor2", originalColor2);

            if (myLeader != null)
            {
                myLeader.RemoveBoidFromSwarm(this);
                myLeader = null;
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