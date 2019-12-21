using UnityEngine;

public class Newontroller : MonoBehaviour
{

    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    public float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;

    Vector3 resetPosition;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]

    // Cached
    Transform cachedTransform;
    Transform target;
    float actSpeed;

    void Start()
    {
        resetPosition = transform.position;
        cachedTransform = transform;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        actSpeed = (minSpeed + maxSpeed) / 2;
        velocity = transform.forward * actSpeed;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            actSpeed -= 0.5f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            actSpeed += 0.5f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            transform.position = resetPosition;
        }


        Vector3 acceleration = Vector3.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * targetWeight;
        }

        // if (numPerceivedFlockmates != 0)
        // {
        //     centreOfFlockmates /= numPerceivedFlockmates;

        //     Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

        //     var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
        //     var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
        //     var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

        //     acceleration += alignmentForce;
        //     acceleration += cohesionForce;
        //     acceleration += seperationForce;
        // }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * avoidCollisionWeight;
            acceleration += collisionAvoidForce;
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
        if (true)
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
        Vector3[] rayDirections = BoidHelper();


        for (int i = 0; i < rayDirections.Length; i++)
        {

            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);

            if (true)
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

    const int numViewDirections = 200;


    static Vector3[] BoidHelper()
    {
        Vector3[] directions = new Vector3[numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }
        return directions;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }
}
