using UnityEngine;

public class DropFood : MonoBehaviour
{
    public Vector3 target;
    public GameObject Food;


    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            for (int i = 0; i < 6; i++)
            {
                target = new Vector3(Random.Range(-20, 20), 5, Random.Range(-20, 20));
                Instantiate(Food, target, Quaternion.identity);
            }
        }
    }
}
