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
        // x
        if(transform.position.x > worldHandler.worldX)
        {
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
            return true;
        }

        if (transform.position.x < 0)
        {
            transform.position = new Vector3(worldHandler.worldX, transform.position.y, transform.position.z);
            return true;
        }

        // z
        if (transform.position.z > worldHandler.worldZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            return true;
        }

        if (transform.position.z < 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, worldHandler.worldZ);
            return true;
        }

        // z
        if (transform.position.y > worldHandler.worldHeight)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            return true;
        }

        if (transform.position.z < 0)
        {
            transform.position = new Vector3(transform.position.x, worldHandler.worldHeight, transform.position.z);
            return true;
        }

        return false;


    }



    void LeaderFollowing()
    {

    }


}
