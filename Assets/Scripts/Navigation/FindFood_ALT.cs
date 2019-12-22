using UnityEngine;

public class FindFood : MonoBehaviour
{
    public Vector3 target;

    private FindNearestGameObject findStuff;
    private bool goingForFood;
    GameObject closestFood = null;
    Color color;


    public float speed = 3.0f;

    void Awake()
    {
        target = transform.position;
        findStuff = FindObjectOfType<FindNearestGameObject>();
        goingForFood = false;
    }

    void LateUpdate()
    {
        if (!goingForFood)
        {
            closestFood = findStuff.FindClosestFood();
            goingForFood = closestFood ? true : false;
            if (goingForFood)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", Color.green);
                closestFood.GetComponent<Renderer>().SetPropertyBlock(block);
            }
        }

        if (closestFood && goingForFood)
        {
            target = closestFood.transform.position;
        }
        else if (goingForFood && !closestFood)
        {
            goingForFood = false;
            closestFood = null;
        }

        transform.position = Vector3.Lerp(transform.position, target, 0.5f * Time.deltaTime * 2);

        Vector3 targetDirection = target - transform.position;
        float singleStep = speed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        Debug.DrawRay(transform.position, newDirection, Color.red);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        goingForFood = false;
        Destroy(closestFood);
        closestFood = null;
    }
}
