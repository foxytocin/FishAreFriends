﻿using UnityEngine;
using Unity.Mathematics;

public class KeyController : MonoBehaviour
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

    // Key events
    bool upKeyPressed = false;
    bool downKeyPressed = false;
    bool leftKeyPressed = false;
    bool rightKeyPressed = false;

    // Debug
    bool showDebug = true;


    void Awake()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Obstacle");
        cellGroups = FindObjectOfType<CellGroups>();
        cachedTransform = transform;
        material = gameObject.transform.GetChild(2).GetComponent<MeshRenderer>().material;
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

        // speed handling
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;


        if (!isHeadingForCollision)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                speed -= 0.5f;
                //Debug.Log("KeyDown Q: " + speed);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                speed += 0.5f;
                //Debug.Log("KeyDown E: " + speed);
            }
        }

        speed = Mathf.Clamp(speed, 0.00001f, maxSpeed);
        velocity = dir * speed;

        // handle key events
        if (!isHeadingForCollision)
        {
            // key ups
            if (Input.GetKeyDown(KeyCode.A))
                leftKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.D))
                rightKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.W))
                upKeyPressed = true;
            if (Input.GetKeyDown(KeyCode.S))
                downKeyPressed = true;
        }

        // key downs
        if (Input.GetKeyUp(KeyCode.A))
            leftKeyPressed = false;
        if (Input.GetKeyUp(KeyCode.D))
            rightKeyPressed = false;
        if (Input.GetKeyUp(KeyCode.W))
            upKeyPressed = false;
        if (Input.GetKeyUp(KeyCode.S))
            downKeyPressed = false;


        if (!isHeadingForCollision)
        {

            if (leftKeyPressed)
                velocity -= transform.right * Time.deltaTime * speed;
            if (rightKeyPressed)
                velocity += transform.right * Time.deltaTime * speed;
            if (upKeyPressed)
                velocity += transform.up * Time.deltaTime * speed;
            if (downKeyPressed)
                velocity -= transform.up * Time.deltaTime * speed;

        }


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
}
