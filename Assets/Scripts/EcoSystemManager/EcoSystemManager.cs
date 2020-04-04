using System.Collections.Generic;
using UnityEngine;

public class EcoSystemManager : MonoBehaviour
{
    MapGenerator mapGenerator;
    FoodManager foodManager;

    [Header("Statistics")]
    public int fishCount;
    public int foodDemandFishes;
    public int availableFood;
    public int diedFishes;


    [Header("Predator")]
    public int killedFishes;
    public int foodLeft;


    [Header("Food Management")]
    public bool feedAutomatically;
    public int ifHungerAbove = 300;
    public int andAvailableFoodBelow = 40000;


    public Queue<Vector3> spawnPoints;
    public Queue<Vector3> initialSpawnPoints;
    private Vector3 mapSize;
    private Vector3 spawnOffset;


    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        mapSize = mapGenerator.mapSize;
        spawnOffset = mapSize * 0.2f;
        spawnPoints = new Queue<Vector3>();
        initialSpawnPoints = new Queue<Vector3>();
        foodManager = FindObjectOfType<FoodManager>();
        availableFood = 0;
        foodDemandFishes = 0;
        fishCount = 0;
        diedFishes = 0;
        killedFishes = 0;
        foodLeft = 0;
        feedAutomatically = true;
    }


    public void setfoodDemandFishes(int amount)
    {
        if (fishCount == 0)
            return;

        foodDemandFishes = amount / fishCount;

        if (feedAutomatically && foodDemandFishes > ifHungerAbove && availableFood < andAvailableFoodBelow)
        {
            foodManager.dropFood();
        }
    }

    public void setAvailableFood(int amount)
    {
        availableFood += amount;
    }


    public void setFishCount(int amount)
    {
        fishCount = amount;
    }

    public void addFishToFishCount()
    {
        fishCount++;
    }

    public void addDiedFish()
    {
        diedFishes++;
        fishCount--;
    }

    public void addKilledFish()
    {
        killedFishes++;
    }

    public void setFoodDemandPredator(int amount)
    {
        foodLeft = amount;
    }

    public void AddSpawnPoint(Vector3 point)
    {
        spawnPoints.Enqueue(point);
    }


    public Vector3 GetNextSpawnPoint()
    {
        Vector3 point;
        if (spawnPoints.Count != 0)
        {
            point = spawnPoints.Dequeue();
            spawnPoints.Enqueue(point);
            return point;
        }

        if (initialSpawnPoints.Count == 0)
        {
            initialSpawnPoints.Enqueue(new Vector3(spawnOffset.x, mapSize.y / 2, spawnOffset.z));
            initialSpawnPoints.Enqueue(new Vector3(mapSize.x - spawnOffset.x, mapSize.y / 2, mapSize.z - spawnOffset.z));
            initialSpawnPoints.Enqueue(new Vector3(spawnOffset.x, mapSize.y / 2, mapSize.z - spawnOffset.z));
            initialSpawnPoints.Enqueue(new Vector3(mapSize.x - spawnOffset.x, mapSize.y / 2, spawnOffset.z));
        }

        point = initialSpawnPoints.Dequeue();
        initialSpawnPoints.Enqueue(point);
        return point;
    }

    public void ResetSpawnPoints() {
        spawnPoints = new Queue<Vector3>();
        initialSpawnPoints = new Queue<Vector3>();
    }
}