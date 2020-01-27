using UnityEngine;
using System.Collections;

public class Food : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    MapGenerator mapGenerator;
    FoodManager foodManager;
    private int availableFood;
    private float size;
    private float destroyHeight = 0;
    Coroutine shrinkAnimation = null;


    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        foodManager = FindObjectOfType<FoodManager>();
        availableFood = Random.Range(2000, 20000);
        ecoSystemManager.setAvailableFood(availableFood);
        size = (float)availableFood / 10000f;

        destroyHeight = mapGenerator.noiseMap[(int)transform.position.x, (int)transform.position.z] * mapGenerator.heightScale;

        StartCoroutine(GrowAnimation());
        StartCoroutine(SinkAnimation());
    }

    float tmp = 0;
    private IEnumerator GrowAnimation()
    {
        while (tmp < size)
        {
            transform.localScale = new Vector3(tmp, tmp, tmp);
            tmp += 0.1f;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator SinkAnimation()
    {
        float poxY = transform.position.y;
        float speed = Random.Range(0.05f, 0.35f);
        while (transform.position.y > destroyHeight)
        {
            poxY = Mathf.Lerp(transform.position.y, 0, 0.1f * Time.deltaTime * speed);
            transform.position = new Vector3(transform.position.x, poxY, transform.position.z);

            if (shrinkAnimation == null && transform.position.y < destroyHeight + 4)
                shrinkAnimation = StartCoroutine("ShrinkAnimation");

            yield return new WaitForEndOfFrame();
        }
    }

    public void SetColor(Color col)
    {
        GetComponent<Renderer>().material.SetColor("_EmissionColor", col * 1.5f);
    }


    private IEnumerator ShrinkAnimation()
    {
        while (availableFood > 200)
        {
            availableFood -= 200;
            ecoSystemManager.setAvailableFood(-200);
            scaleFood();
            yield return new WaitForEndOfFrame();
        }

        ecoSystemManager.setAvailableFood(-availableFood);
        StartCoroutine(Destroy());
    }


    public void Explode()
    {
        tmp = size - 0.2f;
        StartCoroutine(ExplodeAnimation());
    }

    private IEnumerator ExplodeAnimation()
    {
        float force = Random.Range(0.2f, 1f);
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), Random.Range(-1f, 1f));
        while (force > 0)
        {
            transform.position += dir * force;
            force -= 0.1f;
            yield return new WaitForEndOfFrame();
        }

        destroyHeight = mapGenerator.noiseMap[(int)transform.position.x, (int)transform.position.z] * mapGenerator.heightScale;
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

        yield return new WaitForSeconds(1);
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