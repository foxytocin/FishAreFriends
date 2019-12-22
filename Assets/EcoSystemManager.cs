using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcoSystemManager : MonoBehaviour
{
    public int fishCount;
    public float foodDemand;
    public float availableFood;
    public int diedFishes;

    // Start is called before the first frame update
    void Awake()
    {
        availableFood = 0;
        foodDemand = 0;
        fishCount = 0;
        diedFishes = 0;
    }


    public void setFoodDemand(float amount)
    {
        foodDemand = amount / fishCount;
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
