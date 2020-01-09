using UnityEngine;
using System.Collections;

public class Food : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    FoodManager foodManager;
    private int availableFood;
    private float size;


    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        foodManager = FindObjectOfType<FoodManager>();
        availableFood = Random.Range(5000, 40000);
        ecoSystemManager.setAvailableFood(availableFood);
        size = (float)availableFood / 10000f;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float tmp = 0;
        while (tmp < size)
        {
            transform.localScale = new Vector3(tmp, tmp, tmp);
            tmp += 0.05f;
            yield return new WaitForEndOfFrame();
        }
    }

    private void scaleFood()
    {
        float size = (float)availableFood / 10000f;
        transform.localScale = new Vector3(size, size, size);
    }

    public int getFood(int amount)
    {
        if (availableFood >= amount)
        {
            availableFood -= amount;
            ecoSystemManager.setAvailableFood(-amount);

            scaleFood();
            return amount;
        }

        if (amount > availableFood)
        {
            int tmp = availableFood;
            availableFood = 0;
            ecoSystemManager.setAvailableFood(-tmp);

            scaleFood();
            StartCoroutine(Destroy());
            return tmp;
        }

        StartCoroutine(Destroy());
        return 0;
    }


    private IEnumerator Destroy()
    {
        RemoveFromFoodList();
        
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    public float checkAmount()
    {
        return availableFood;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void RemoveFromFoodList()
    {
        foodManager.RemoveFromList(this);
    }
}