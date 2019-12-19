using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{

    private ObjectPooler _objectPooler;

    private int _spawnedObjects = 9;

    // Start is called before the first frame update
    void Start()
    {
        _objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void Update()
    {

        if(_spawnedObjects-- > 0)
        {
            _objectPooler.SpawnObjectFromPool("Fish", new Vector3(Random.Range(0, 150), Random.Range(2, 10), Random.Range(0, 150)), Quaternion.identity);

        }


    }
}
