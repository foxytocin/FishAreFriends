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
    private Leader myLeaderScript;
    private Boid boidToHunt = null;
    private Leader leaderToAttack = null;
    private float timeToStayNextToAttackedLeader;
    private float timeToRehunt;

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
        myLeaderScript = gameObject.GetComponent<Leader>();
        timeToStayNextToAttackedLeader = 0f;
        timeToRehunt = 0;
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
        // just for debuging
        if (Input.GetKeyDown(KeyCode.U))
            opponentBehavior = OpponentBehavior.SearchForBoids;
        if (Input.GetKeyDown(KeyCode.I))
            opponentBehavior = OpponentBehavior.AttackOtherLeader;
        if (Input.GetKeyDown(KeyCode.O))
            opponentBehavior = OpponentBehavior.SearchForFood;

        // end


        if(myLeaderScript.GetSwarmSize() < 300)
        {
            opponentBehavior = OpponentBehavior.SearchForBoids;
            //Debug.Log("Switched opponentBehavior to searchForBoids");
        }
        else
        {
            opponentBehavior = OpponentBehavior.AttackOtherLeader;
            Debug.Log("Switched opponentBehavior to attackOtherLeader");
        }


        // reset variables
        if (!opponentBehavior.Equals(OpponentBehavior.SearchForBoids))
            boidToHunt = null;

        if (!opponentBehavior.Equals(OpponentBehavior.AttackOtherLeader))
        {
            leaderToAttack = null;
            timeToStayNextToAttackedLeader = 0;
            timeToRehunt = 0;
        }
      

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

            //////////////////////////////
            ////// search for boids //////
            //////////////////////////////

            // search for boids to join them to your swarm
            if (boidToHunt == null && opponentBehavior.Equals(OpponentBehavior.SearchForBoids))
            {
                List<Boid> boidsNearby = cellGroups.allBoidCells[cellGroups.GetIndex(transform.position)];

                for (int i = 0; i < boidsNearby.Count; i++)
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




            ////////////////////////////////
            ////// attack other swarm //////
            ////////////////////////////////
            if (opponentBehavior.Equals(OpponentBehavior.AttackOtherLeader))
            {


                // set time, that avoid attacking swarm again and again
                timeToRehunt -= Time.deltaTime;


                // if i have no leader to hunt
                if (leaderToAttack == null && timeToRehunt <= 0)
                {
                    Debug.Log("Search for other leader");
                    // find other leader
                    float distanceToLeader = float.MaxValue;
                    if (Leader.leaderList != null)
                    {
                        foreach (Leader leader in Leader.leaderList)
                        {
                            if (leader.Equals(myLeaderScript))
                                continue;

                            float tempDistance = Vector3.Distance(position, leader.getPosition());
                            if (tempDistance < distanceToLeader)
                            {
                                leaderToAttack = leader;
                                distanceToLeader = tempDistance;
                                Debug.Log("I found an other leader, but check his strength.");
                            }
                        }

                        // if the nearest leaders swarm is smaler than mine, than attack im
                        if (distanceToLeader < 80f && leaderToAttack.GetSwarmSize() < myLeaderScript.GetSwarmSize())
                        {
                            timeToStayNextToAttackedLeader = 7f;
                            Debug.Log("Strength is ok. Lets attack him.");
                        }
                        else
                        {
                            // if other swarm is too big, search for boids
                            Debug.Log("The other swarm is to strong for me. I search for boids.");
                            opponentBehavior = OpponentBehavior.SearchForBoids;
                            leaderToAttack = null;
                        }
                    }
                }
                else if (leaderToAttack != null)
                {
                    // if i have a leader to attack
                    acceleration += leaderToAttack.transform.position - position;

                    // if i got some boids, go away from this leader
                    float distanceToOtherLeader = Vector3.Distance(leaderToAttack.transform.position, position);
                    if (distanceToOtherLeader < 5f && timeToStayNextToAttackedLeader > 0)
                    {
                        // stay next to leader a specific time
                        timeToStayNextToAttackedLeader -= Time.deltaTime;
                        Debug.Log("I came to the other leader less than 5 EE.");

                    }
                    else if (distanceToOtherLeader < 10f && timeToStayNextToAttackedLeader <= 0)
                    {
                        // go away from other leader
                        acceleration += position ;
                    }
                    else if (distanceToOtherLeader > 10f && timeToStayNextToAttackedLeader <= 0)
                    {
                        Debug.Log("I can search for a new leader to attack.");
                        // delete leaderToAttack, if i am fa
                        leaderToAttack = null;
                        speed = maxSpeed / 3;
                        timeToRehunt = 15f;
                    }
                }




            }


            /////////////////////////////
            ////// search for food //////
            /////////////////////////////
            ///
            if (opponentBehavior.Equals(OpponentBehavior.SearchForFood))
            {

            }



        }


        velocity += acceleration * Time.deltaTime;

        // if speed not set befor
        if (speed == 0)
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
