using UnityEngine;

public class KeyController : MonoBehaviour
{
    public float maxSpeed = 3;
    public float maxPitchSpeed = 30;
    public float maxTurnSpeed = 70;
    public float acceleration = 2;

    public float smoothSpeed = 3;
    public float smoothTurnSpeed = 3;


    Vector3 velocity;
    float yawVelocity;
    float pitchVelocity;
    float currentSpeed;


    void Start()
    {
        currentSpeed = maxSpeed;
    }

    void Update()
    {
        float accelDir = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            accelDir -= 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            accelDir += 1;
        }
        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector3(0, 0, 0);
        }

        currentSpeed += acceleration * Time.deltaTime * accelDir;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        float speedPercent = currentSpeed / maxSpeed;

        Vector3 targetVelocity = transform.forward * currentSpeed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * smoothSpeed);

        float targetPitchVelocity = Input.GetAxisRaw("Vertical") * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = Input.GetAxisRaw("Horizontal") * maxTurnSpeed;
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
    }
}
