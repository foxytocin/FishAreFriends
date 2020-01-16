using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    MapGenerator mapGenerator;
    private Vector3 spawnPoint;
    public Food foodPrefab;
    private Vector3 spawnOffset;
    private Vector3 mapSize;
    Transform foodHolder;
    public static List<Food> foodList;

    void Awake()
    {
        foodList = new List<Food>();
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

        Food food = Instantiate(foodPrefab, spawnPoint, Quaternion.identity);
        foodList.Add(food);
        food.transform.parent = foodHolder;
    }

    public void RemoveFromList(Food food)
    {
        foodList.Remove(food);
    }
}
