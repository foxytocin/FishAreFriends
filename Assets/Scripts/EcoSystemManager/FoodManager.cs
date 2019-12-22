using UnityEngine;

public class FoodManager : MonoBehaviour
{
    private Vector3 target;
    public GameObject Food;

    Transform foodHolder;

    void Awake()
    {
        foodHolder = new GameObject("Food").transform;
    }

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
        GameObject go = Instantiate(Food, target, Quaternion.identity);
        go.transform.parent = foodHolder;
    }
}
