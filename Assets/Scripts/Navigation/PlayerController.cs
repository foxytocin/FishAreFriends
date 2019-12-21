using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public class PlayerController : MonoBehaviour
{

    public float speed;
    Vector3 velocity;

    Vector3 position;
    Transform cachedTransform;
    public Vector3 forward;
    public float minSpeed = 0f;
    public float maxSpeed = 3f;
    public float maxSteerForce = 5f;
    public Vector3 target;
    public float targetWeight = 100f;
    public float alignWeight = 100f;


    // Start is called before the first frame update
    void Start()
    {
        target = Vector3.zero;
        cachedTransform = transform;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (maxSpeed - minSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("HIT-POINT: " + hit.point);
                target = hit.point + new Vector3(0, 2, 0);
            }
        }




        //Inputs
        if (Input.GetAxis("Horizontal") > 0)
        {
            velocity += new Vector3(1, 0, 0);
            Debug.Log("Horizontal");
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            velocity += new Vector3(-1, 0, 0);
            Debug.Log("Horizontal");
        }

        if (Input.GetAxis("Vertical") > 0)
        {
            velocity = new Vector3(0, 1, 0);
            Debug.Log("Vetical");
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            velocity = new Vector3(0, -1, 0);
            Debug.Log("Vetical");
        }


        // Berechnung der Bewegung
        Vector3 acceleration = Vector3.zero;

        if (target != transform.position)
        {
            Vector3 offsetToTarget = (target - position);
            acceleration = SteerTowards(offsetToTarget) * targetWeight;


            var alignmentForce = SteerTowards(target) * alignWeight;

            acceleration += alignmentForce;
            velocity += acceleration;
            position += velocity;


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
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }
}
