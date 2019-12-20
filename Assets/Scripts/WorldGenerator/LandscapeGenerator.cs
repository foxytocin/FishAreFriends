using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{


    public GameObject prefabCube;
    public GameObject prefabCylinder;
    public int amountSzeneElements;

    void Awake()
    {
        for (int i = 0; i < amountSzeneElements; i++)
        {
            GameObject instName = (Random.Range(0, 2) == 0) ? prefabCube : prefabCylinder;
            float elementHeight = Random.Range(4f, 15f);
            float elementWidth = Random.Range(2f, 6f);

            Vector2 pos = new Vector2(Random.Range(-26, 26), Random.Range(-26, 26));

            GameObject newObject = Instantiate(instName, new Vector3(pos[0], elementHeight / 2f, pos[1]), Quaternion.identity) as GameObject;
            newObject.transform.localScale = new Vector3(elementWidth, elementHeight, elementWidth);
        }
    }

}
