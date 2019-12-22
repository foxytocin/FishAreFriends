using UnityEngine;

public class GrassGenerator : MonoBehaviour
{

    public float size;
    public float resolution;
    public GameObject prefab;

    private float stepSize;
    public float noiseScale;

    public float grassScale;

    public GameObject grassPrefab;
    public float thresholdGrass;

    private Vector2 offset;

    Renderer m_Renderer;

    // Start is called before the first frame update
    void Start()
    {
        stepSize = size / resolution;
        offset = new Vector2(transform.position.x, transform.position.z);
        generateGround();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void generateGround()
    {
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
                    GameObject grass = Instantiate(grassPrefab, new Vector3(i + offset[0], 1, j + offset[1]), Quaternion.identity);
                    float gs = sample * grassScale;
                    grass.transform.localScale = new Vector3(gs, gs, gs);
                    grass.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                }
            }
        }
    }

    int seed = Random.Range(0, 100000);
    private float groundType(float i, float j)
    {
        float xCoord = seed + i / size * noiseScale;
        float yCoord = seed + j / size * noiseScale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        //Debug.Log("Value: " + sample);
        return sample;
    }
}
