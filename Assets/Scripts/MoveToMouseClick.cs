using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToMouseClick : MonoBehaviour
{

    private Vector3 target;
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
                Debug.Log("HIT-POINT: " + hit.point);
                target = hit.point + new Vector3(0, 2, 0);
            }
        }

        transform.position = Vector3.Lerp(transform.position, target, 0.5f * Time.deltaTime);
    }
}
