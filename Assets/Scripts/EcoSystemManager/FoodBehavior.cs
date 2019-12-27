﻿using UnityEngine;
using System.Collections;

public class FoodBehavior : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    private int availableFood;
    private BoxCollider b_collider;
    private float size;


    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        availableFood = Random.Range(5000, 40000);
        ecoSystemManager.setAvailableFood(availableFood);
        size = (float)availableFood / 10000f;
        b_collider = GetComponent<BoxCollider>();
        b_collider.enabled = false;
        b_collider.size = new Vector3(size, size, size);
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
        b_collider.enabled = true;
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
        gameObject.layer = 2;
        b_collider.enabled = false;
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    public float checkAmount()
    {
        return availableFood;
    }
}