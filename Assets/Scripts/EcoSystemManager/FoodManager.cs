using UnityEngine;

public class FoodManager : MonoBehaviour
{
    private Vector3 target;
    public GameObject Food;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            dropFood();
        }
    }

    public void dropFood()
    {
        target = new Vector3(Random.Range(-20, 20), Random.Range(3, 15), Random.Range(-20, 20));
        Instantiate(Food, target, Quaternion.identity);
    }
}
