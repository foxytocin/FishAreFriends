using UnityEngine;

public class CamTopView : MonoBehaviour
{

    MapGenerator mapGenerator;

    // Start is called before the first frame update
    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        centerOverTank();
    }

    private void centerOverTank()
    {
        Vector3 center = mapGenerator.mapSize / 2;
        center += new Vector3(0, mapGenerator.mapSize.y / 2, 0);
        transform.position = center;
    }
}
