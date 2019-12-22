using UnityEngine;

public class FoodBehavior : MonoBehaviour
{

    private float availableFood;
    private BoxCollider b_collider;


    void Awake()
    {
        availableFood = Random.Range(500, 5000);
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
            scaleFood();
            Debug.Log("FOOD: Ordert: " + amount + " / Received: " + amount);
            return amount;
        }

        if (amount > availableFood)
        {
            float tmp = availableFood;
            availableFood = 0f;
            scaleFood();
            Debug.Log("FOOD: Ordert: " + amount + " / Received: " + tmp);
            gameObject.layer = 2;
            return tmp;
        }

        gameObject.layer = 2;
        return 0f;
    }

    public float checkAmount()
    {
        return availableFood;
    }



}
