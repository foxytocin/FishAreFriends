using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcoSystemManager : MonoBehaviour
{

    FoodManager foodManager;

    [Header("Statistics")]
    public int fishCount;
    public float foodDemand;
    public float availableFood;
    public int diedFishes;

    [Header("Food Management")]
    public bool feedAutomatically;
    public int ifHungerAbove = 50;
    public int andAvailableFoodBelow = 1000;

    // Start is called before the first frame update
    void Awake()
    {
        foodManager = FindObjectOfType<FoodManager>();
        availableFood = 0;
        foodDemand = 0;
        fishCount = 0;
        diedFishes = 0;
        feedAutomatically = true;
    }


    public void setFoodDemand(float amount)
    {
        foodDemand = amount / fishCount;

        if (feedAutomatically && foodDemand > ifHungerAbove && availableFood < andAvailableFoodBelow)
        {
            foodManager.dropFood();
        }
    }

    public void setAvailableFood(float amount)
    {
        availableFood += amount;
    }

    public void setFishCount(int amount)
    {
        fishCount = amount;
    }

    public void addDiedFish()
    {
        diedFishes++;
        fishCount--;
    }
}
