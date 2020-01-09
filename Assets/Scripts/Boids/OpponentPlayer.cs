using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class OpponentPlayer : MonoBehaviour
{

    CellGroups cellGroups;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    Transform cachedTransform;
    Material material;

    private float maxSpeed = 3f;
    public float maxSteerForce = 2f;
    public float avoidCollisionWeight = 10f;
    public float boundsRadius = 0.27f;
    public float collisionAvoidDst = 5f;
    public LayerMask obstacleMask;


    private OpponentBehavior opponentBehavior;
    private Boid boidToHunt = null;

    // swarm search, food search enum
    public enum OpponentBehavior
    {
        SearchForBoids,
        SearchForFood,
        AttackOtherLeader
    }


    void Awake()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Obstacle");
        cellGroups = FindObjectOfType<CellGroups>();
        cachedTransform = transform;
        material = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        opponentBehavior = OpponentBehavior.SearchForBoids;
    }

    public void Start()
    {

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = maxSpeed / 2;
        velocity = transform.forward * startSpeed;
    }

    void Update()
    {

        cellGroups.SetPlayerCell(transform.position);

        Vector3 acceleration = Vector3.zero;
        bool isHeadingForCollision = IsHeadingForCollision();

        // avoid collisiton
        if (isHeadingForCollision)
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }


        float speed = 0;
        if (!isHeadingForCollision)
        {
            if (boidToHunt == null && opponentBehavior.Equals(OpponentBehavior.SearchForBoids))
            {
                List<Boid> boidsNearby = cellGroups.allBoidCells[cellGroups.GetIndex(transform.position)];

                for(int i = 0; i < boidsNearby.Count; i++)
                {
                    if (!boidsNearby[i].HasLeader())
                    {
                        boidToHunt = boidsNearby[i];
                        speed = maxSpeed;
                        //Debug.Log("Hunt boid");
                    }
                }
            }

            if (boidToHunt != null)
            {
                acceleration += boidToHunt.transform.position - position;
                if (boidToHunt.HasLeader())
                {
                    boidToHunt = null;
                    Debug.Log("Boid reached or joined to other swarm.");
                    speed = maxSpeed / 4;
                }
                    
            }

        }



        velocity += acceleration * Time.deltaTime;

        // if speed not set befor
        if(speed == 0)
             speed = velocity.magnitude;

        Vector3 dir = velocity / speed;



        speed = Mathf.Clamp(speed, 0.00001f, maxSpeed);
        velocity = dir * speed;



        

        


        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;

        position = cachedTransform.position;
        forward = dir;

        float ws = material.GetFloat("_WobbleSpeed");
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
        for (int i = 0; i < 2; i++)
        {
            material.SetFloat("_WobbleSpeed", speed);
        }
    }


    bool IsHeadingForCollision()
    {

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
