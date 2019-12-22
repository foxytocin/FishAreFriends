using UnityEngine;

public class FoodBehavior : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    private int availableFood;
    private BoxCollider b_collider;


    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        availableFood = Random.Range(5000, 40000);
        ecoSystemManager.setAvailableFood(availableFood);
        b_collider = GetComponent<BoxCollider>();
        scaleFood();
    }


    private void scaleFood()
    {
        float size = (float)availableFood / 10000f;
        transform.localScale = new Vector3(size, size, size);
        b_collider.size = new Vector3(size, size, size);
    }

    public int getFood(int amount)
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
            int tmp = availableFood;
            availableFood = 0;
            ecoSystemManager.setAvailableFood(-tmp);
            scaleFood();
            //Debug.Log("FOOD: Ordert: " + amount + " / Received: " + tmp);
            gameObject.layer = 2;
            gameObject.SetActive(false);
            return tmp;
        }

        gameObject.layer = 2;
        gameObject.SetActive(false);
        return 0;
    }

    public float checkAmount()
    {
        return availableFood;
    }



}
