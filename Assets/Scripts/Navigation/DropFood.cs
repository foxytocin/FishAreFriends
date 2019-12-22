using UnityEngine;

public class DropFood : MonoBehaviour
{
    public Vector3 target;
    public GameObject Food;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            for (int i = 0; i < 3; i++)
            {
                target = new Vector3(Random.Range(-20, 20), Random.Range(3, 15), Random.Range(-20, 20));
                Instantiate(Food, target, Quaternion.identity);
            }
        }
    }
}
