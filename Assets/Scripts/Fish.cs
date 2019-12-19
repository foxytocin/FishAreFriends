using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    GameObject leader = null;

    WorldHandler worldHandler;

    float _timeToChangeDirection = 100;
    int _movement = 40;
    int _height = 3;
    int _speed = 1;


    float lastX = 0;
    float lastY = 0;
    float lastZ = 0;

    public void Start()
    {
       worldHandler = WorldHandler.Instance;
    }

    // Update is called once per frame
    public void Update()
    {

        if (keepInBorders())
        {
            return;
        }
        
        transform.Rotate(new Vector3(0, _movement, _height) * Time.deltaTime);

        //Forward Movement
        transform.Translate(new Vector3(_speed, 0, 0) * Time.deltaTime);

        _timeToChangeDirection -= Random.Range(1, 2);

        if (_timeToChangeDirection < 0)
        {
            _timeToChangeDirection = 100;

            _movement = Random.Range(-60, 60);

            if (_speed > 50)
                _speed = 3;
            _speed = Random.Range(_speed - 3, _speed + 5);


            _height = Random.Range(-1, 1);

            
        }

    }

    private bool keepInBorders()
    {

        if(transform.position.x < 0 || transform.position.x > worldHandler.worldX)
        {
            transform.forward = transform.forward * -1;
        }

        if (transform.position.y < 0 || transform.position.y > worldHandler.worldHeight)
        {
            transform.forward = transform.forward * -1;
        }

        if (transform.position.z < 0 || transform.position.z > worldHandler.worldZ)
        {
            transform.forward = transform.forward * -1;
        }


        return false;
    }



    void LeaderFollowing()
    {

    }


}
