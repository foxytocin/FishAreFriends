using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcoSystemManager : MonoBehaviour
{

    FoodManager foodManager;

    [Header("Statistics")]
    public int fishCount;
    public int foodDemand;
    public int availableFood;
    public int diedFishes;

    [Header("Food Management")]
    public bool feedAutomatically;
    public int ifHungerAbove = 300;
    public int andAvailableFoodBelow = 40000;

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


    public void setFoodDemand(int amount)
    {
        foodDemand = amount / fishCount;

        if (feedAutomatically && foodDemand > ifHungerAbove && availableFood < andAvailableFoodBelow)
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

    public void addDiedFish()
    {
        diedFishes++;
        fishCount--;
    }
}
