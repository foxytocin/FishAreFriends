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
    public AudioSource audioSource;
    private Color[] randomColor;

    void Awake()
    {
        foodList = new List<Food>();
        foodHolder = new GameObject("Food").transform;
        audioSource = GetComponent<AudioSource>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        mapSize = mapGenerator.mapSize;
        spawnOffset = mapSize * 0.06f;
        randomColor = new Color[] { Color.yellow }; //{ Color.red, Color.green, Color.blue, Color.yellow };
    }


    public void dropFood()
    {

        Color col = randomColor[Random.Range(0, randomColor.Length)];
        audioSource.Play();

        float x = Random.Range(spawnOffset.x, mapSize.x - spawnOffset.x);
        float y = Random.Range(mapSize.y * 0.7f + spawnOffset.y, mapSize.y - 2f);
        float z = Random.Range(spawnOffset.z, mapSize.z - spawnOffset.z);
        spawnPoint = new Vector3(x, y, z);

        // main food-source
        Food food = Instantiate(foodPrefab, spawnPoint, Quaternion.identity);
        foodList.Add(food);
        food.SetColor(col);
        food.transform.parent = foodHolder;

        for (int i = 0; i < 8; i++)
        {
            Food foodChild = Instantiate(foodPrefab, spawnPoint, Quaternion.identity);
            foodList.Add(foodChild);
            foodChild.SetColor(col);
            foodChild.Explode();
            foodChild.transform.parent = foodHolder;
        }
    }

    public void RemoveFromList(Food food)
    {
        foodList.Remove(food);
    }
}
