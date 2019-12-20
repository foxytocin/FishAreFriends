using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{


    public Transform follow;


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, follow.position, 0.3f);
    }
}
