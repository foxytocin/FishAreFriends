using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    private Vector3 startPos;

    public float speed = 0.5f;
    public float xScale = 4;

    public float zScale = 4;
    public float yScale = 2;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.eulerAngles += new Vector3(0, 1f, 0);
        transform.position = startPos + (Vector3.right * Mathf.Sin(Time.timeSinceLevelLoad / 2 * speed) * xScale - Vector3.forward * Mathf.Sin(Time.timeSinceLevelLoad * speed) * zScale - -Vector3.up * Mathf.Sin(Time.timeSinceLevelLoad * speed) * yScale);
    }
}
