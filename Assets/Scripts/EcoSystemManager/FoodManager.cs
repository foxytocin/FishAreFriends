using UnityEngine;

public class FoodManager : MonoBehaviour
{
    private Vector3 target;
    public GameObject Food;
    public int saveArea;

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
        target = new Vector3(Random.Range(0 + saveArea, 100 - saveArea), Random.Range(0 + saveArea, 30 - saveArea), Random.Range(0 + saveArea, 100 - saveArea));
        GameObject go = Instantiate(Food, target, Quaternion.identity);
        go.transform.parent = foodHolder;
    }
}
