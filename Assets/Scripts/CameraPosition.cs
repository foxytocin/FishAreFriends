using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{


    public Transform Top_View;
    public Transform Side_View;
    public Transform target;

    bool side = true;

    private Transform targetPosition;


    // Start is called before the first frame update
    void Start()
    {
        targetPosition = Side_View;
        //transform.position = targetPosition.position;
    }

    // Update is called once per frame
    void Update()
    {
        bool down = Input.GetKeyDown(KeyCode.Space);
        if (down & side)
        {
            side = false;
            targetPosition = Top_View;
        }
        else if (down & !side)
        {
            side = true;
            targetPosition = Side_View;
        }

        if (targetPosition.position != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, 0.15f);
        }

        transform.LookAt(target);
    }

}
