using UnityEngine;

public class FoodManager : MonoBehaviour
{
    MapGenerator mapGenerator;
    private Vector3 spawnPoint;
    public GameObject Food;
    private Vector3 spawnOffset;
    private Vector3 mapSize;
    Transform foodHolder;

    void Awake()
    {
        foodHolder = new GameObject("Food").transform;
        mapGenerator = FindObjectOfType<MapGenerator>();
        mapSize = mapGenerator.mapSize;
        spawnOffset = mapSize * 0.2f;
    }

    public void dropFood()
    {
        float x = Random.Range(spawnOffset.x, mapSize.x - spawnOffset.x);
        float y = Random.Range(mapGenerator.heightScale + spawnOffset.y, mapSize.y - spawnOffset.y);
        float z = Random.Range(spawnOffset.z, mapSize.z - spawnOffset.z);
        spawnPoint = new Vector3(x, y, z);

        GameObject go = Instantiate(Food, spawnPoint, Quaternion.identity);
        go.transform.parent = foodHolder;
    }
}
