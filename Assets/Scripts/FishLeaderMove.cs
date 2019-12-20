using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishLeaderMove : MonoBehaviour
{

    private bool _forwardPressed = false;

    public float maximumSpeed = 0.9f;

    public float speedH = 1.0f;
    public float speedV = 1.0f;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private float _speed = 0;
    private float _absorptionValue = 0.008f;


    WorldHandler worldHandler;

    private int _waitLatency = 0;
    public Color leaderColor = Color.green;


    private void Start()
    {
        worldHandler = WorldHandler.Instance;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            _forwardPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            _forwardPressed = false;
        }

        if (_forwardPressed)
        {
            if(_speed < maximumSpeed)
                _speed += 0.01f;
            transform.position += Camera.main.transform.forward * _speed * Time.deltaTime * 10;
        }
        else
        {
            if (_speed > 0)
                _speed -= _absorptionValue;
            transform.position += Camera.main.transform.forward * _speed * Time.deltaTime * 10;
        }


        _yaw += speedH * Input.GetAxis("Mouse X") * 7;
        _pitch -= speedV * Input.GetAxis("Mouse Y") * 7;

        transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
                          
    }


}
