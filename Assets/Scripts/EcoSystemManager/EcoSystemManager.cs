using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcoSystemManager : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Awake()
    {
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
}
