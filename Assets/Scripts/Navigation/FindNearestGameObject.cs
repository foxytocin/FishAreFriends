using UnityEngine;


public class FindNearestGameObject : MonoBehaviour
{
    public GameObject FindClosestFood()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Food");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        if (closest)
        {
            closest.gameObject.tag = "Eaten";
            Debug.Log(closest.transform.position);
        }

        return closest;
    }
}
