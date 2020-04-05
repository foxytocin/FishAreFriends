﻿using UnityEngine;
using Unity.Mathematics;
using System.Collections;

public class KeyController : MonoBehaviour
{

    FlowStream flowStream;
    CellGroups cellGroups;
    GuiOverlay guiOverlay;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    Transform cachedTransform;
    Material material;

    public float maxSpeed = 3f;
    public float maxSteerForce = 2f;
    public float avoidCollisionWeight = 10f;
    public float boundsRadius = 0.27f;
    public float collisionAvoidDst = 5f;
    public LayerMask obstacleMask;

    // Key events
    private bool upKeyPressed = false;
    private bool downKeyPressed = false;
    private bool leftKeyPressed = false;
    private bool rightKeyPressed = false;
    private bool invertedControlls = false;
    private bool invertingSemaphor = false;
    private SettingInvertControlls settingInvertControlls;

    // Debug
    private bool showDebug = false;


    void Awake()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Obstacle");
        cellGroups = FindObjectOfType<CellGroups>();
        guiOverlay = FindObjectOfType<GuiOverlay>();
        flowStream = FindObjectOfType<FlowStream>();
        settingInvertControlls = FindObjectOfType<SettingInvertControlls>();
        cachedTransform = transform;
        material = gameObject.transform.GetChild(2).GetComponent<MeshRenderer>().material;
        Cursor.visible = false;
    }

    public void Start()
    {
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = maxSpeed / 2;
        velocity = transform.forward * startSpeed;
    }


    public void InvertedControlls(bool value)
    {
        invertedControlls = value;
    }

    public bool CheckInvertedControlls()
    {
        return invertedControlls;
    }


    private IEnumerator InvertControll()
    {
        invertingSemaphor = true;

        if (invertedControlls)
        {
            invertedControlls = false;
            settingInvertControlls.SetInvertedControlls(false);
            guiOverlay.DisplayMainMessage("Invertierte Steuerung deaktiviert", 1, GuiOverlay.MessageType.info);
        }
        else
        {
            invertedControlls = true;
            settingInvertControlls.SetInvertedControlls(true);
            guiOverlay.DisplayMainMessage("Invertierte Steuerung aktiviert", 1, GuiOverlay.MessageType.info);
        }

        upKeyPressed = false;
        downKeyPressed = false;
        yield return new WaitForSeconds(0.5f);
        invertingSemaphor = false;
    }


    void Update()
    {
        if (guiOverlay.gameStatus == GuiOverlay.GameStatus.inGame)
        {

            cellGroups.SetPlayerCell(transform.position);

            if (!invertingSemaphor && Input.GetKeyDown(KeyCode.I))
                StartCoroutine(InvertControll());

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
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    leftKeyPressed = true;
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    rightKeyPressed = true;

                if (!invertedControlls)
                {
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                        upKeyPressed = true;
                    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                        downKeyPressed = true;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                        upKeyPressed = true;
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                        downKeyPressed = true;
                }
            }

            // key downs
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
                leftKeyPressed = false;
            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
                rightKeyPressed = false;

            if (!invertedControlls)
            {
                if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                    upKeyPressed = false;
                if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
                    downKeyPressed = false;
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
                    upKeyPressed = false;
                if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                    downKeyPressed = false;
            }

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
    }


    public void setWobbleSpeed(float speed)
    {
        for (int i = 0; i < 2; i++)
        {
            material.SetFloat("_WobbleSpeed", speed);
        }
    }


    Vector3 hitPoint;
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
            //Debug.Log("HIT: " + hit.point);
            hitPoint = hit.point;
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

                Vector3 forward = transform.TransformDirection(Vector3.forward);

                flowStream.playFlowStream(forward, hitPoint);
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
