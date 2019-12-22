using UnityEngine;

public class FoodBehavior : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    private float availableFood;
    private BoxCollider b_collider;


    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        availableFood = Random.Range(500, 5000);
        ecoSystemManager.setAvailableFood(availableFood);
        b_collider = GetComponent<BoxCollider>();
        scaleFood();
    }


    private void scaleFood()
    {
        float size = availableFood / 1000f;
        transform.localScale = new Vector3(size, size, size);
        b_collider.size = new Vector3(size, size, size);
    }

    public float getFood(float amount)
    {
        if (availableFood >= amount)
        {
            availableFood -= amount;
            ecoSystemManager.setAvailableFood(-amount);
            scaleFood();
            //Debug.Log("FOOD: Ordert: " + amount + " / Received: " + amount);
            return amount;
        }

        if (amount > availableFood)
        {
            float tmp = availableFood;
            availableFood = 0f;
            ecoSystemManager.setAvailableFood(-tmp);
            scaleFood();
            //Debug.Log("FOOD: Ordert: " + amount + " / Received: " + tmp);
            gameObject.layer = 2;
            gameObject.SetActive(false);
            return tmp;
        }

        gameObject.layer = 2;
        gameObject.SetActive(false);
        return 0f;
    }

    public float checkAmount()
    {
        return availableFood;
    }



}
