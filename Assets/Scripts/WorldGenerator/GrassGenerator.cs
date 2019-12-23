using UnityEngine;

public class GrassGenerator : MonoBehaviour
{

    public float size;
    public float resolution;
    private float stepSize;
    public float noiseScale;
    public GameObject prefabGrass;
    public GameObject prefabSeaweed;
    public float thresholdGrass;
    public float grassScale;
    public float thresholdSeaweed;
    public float seaweedScale;
    private Vector2 offset;
    private float seed;
    private EcoSystemManager ecoSystemManager;

    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        seed = Random.Range(0, 100000);
        stepSize = size / resolution;
        offset = new Vector2(transform.position.x, transform.position.z);
    }

    // Start is called before the first frame update
    void Start()
    {
        generateGround();
    }


    void generateGround()
    {
        var grassHolder = new GameObject("GrassAndSeaweed").transform;
        for (float i = 0; i < size; i += stepSize)
        {
            for (float j = 0; j < size; j += stepSize)
            {
                //GameObject go = Instantiate(prefab, new Vector3(i + offset[0], 0, j + offset[1]), Quaternion.identity);
                float sample = groundType(i, j);

                //Color col = Color.HSVToRGB(0, 0, sample);
                //go.GetComponent<Renderer>().material.SetColor("_BaseColor", col);

                if (sample > thresholdGrass)
                {
                    GameObject grass = Instantiate(prefabGrass, new Vector3(i + offset[0], 1, j + offset[1]), Quaternion.identity);
                    float gs = sample * grassScale;
                    grass.transform.localScale = new Vector3(gs, gs, gs);
                    grass.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    grass.transform.parent = grassHolder;
                }

                if (sample < thresholdSeaweed)
                {
                    Vector3 grasPosition = new Vector3(i + offset[0], 1, j + offset[1]);
                    GameObject grass = Instantiate(prefabSeaweed, grasPosition, Quaternion.identity);
                    float gs = sample * seaweedScale;
                    grass.transform.localScale = new Vector3(gs, gs * 10, gs);
                    grass.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    grass.transform.parent = grassHolder;

                    // add gras as spawnpoint
                    ecoSystemManager.AddSpawnPoint(grasPosition);

                }
            }
        }
    }


    private float groundType(float i, float j)
    {
        float xCoord = seed + i / size * noiseScale;
        float yCoord = seed + j / size * noiseScale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        //Debug.Log("Value: " + sample);
        return sample;
    }
}
