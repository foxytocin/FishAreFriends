using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToMouseClick : MonoBehaviour
{
    public Vector3 target;

    // Angular speed in radians per sec.
    public float speed = 3.0f;



    void Update()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
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

        transform.position = Vector3.Lerp(transform.position, target, 0.5f * Time.deltaTime);


        // Determine which direction to rotate towards
        Vector3 targetDirection = target - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);

    }
}
