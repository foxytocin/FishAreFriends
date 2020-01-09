using UnityEngine;
using Unity.Collections;

[System.Serializable]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGenerator : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;
    public GameObject underwaterDust;
    public bool generateGras = true;
    public bool generateStones = true;
    public bool generateBricks = true;
    public bool generateWater = true;
    public bool generateGround = true;
    public bool generateWalls = true;


    [Header("Bricks")]
    public GameObject prefabCube;
    public GameObject prefabCylinder;
    public int amountSzeneElements = 30;
    public int sizeVariationBrick = 3;

    [Header("Stones")]
    public GameObject[] stones = new GameObject[5];

    [Range(0, 1f)]
    public float placingDensityStones = 0.4f;
    public int sizeVariation = 8;
    [Range(0, 0.5f)]
    public float groupingStones = 0.3f;



    [Header("Grass")]
    public GameObject prefabGrass;

    [Range(0, 0.5f)]
    public float thresholdGrass = 0.364f;
    public float grassScale = 13;
    [Range(0, 1f)]
    public float growDensityGrass = 0.25f;


    [Header("Seaweed")]
    public GameObject prefabSeaweed;
    [Range(0.5f, 1)]
    public float thresholdSeaweed = 0.835f;
    public float seaweedScale = 3;
    [Range(0, 1f)]
    public float growDensitySeaweed = 0.25f;


    [Header("General World Settings")]
    public GameObject prefabWall;
    public int paddingToMapBorder;
    public bool autoUpdate = true;
    Transform enviromentHolder;
    public int mapResolution = 1;
    public Vector3 mapSize = new Vector3(100, 50, 100);
    public float noiseScale = 50;
    public int octaves = 8;

    [Range(0, 1)]
    public float persistence = 0.49f;
    public float lacunarity = 2;
    public int seed;
    public bool randomSeed = true;
    public Vector2 offset;

    [Header("Ground Setting")]
    public bool debug = false;

    [Range(0, 100f)]
    public float heightScale;
    public Material groundMaterial;
    public float groundResolution = 0.25f;
    private int stepXGround;
    private int stepYGround;
    public Gradient gradient = new Gradient();
    private float minHeight;
    private float maxHeight;


    [Header("Water Setting")]

    public Material waterMaterial;
    public float waterResolution = 0.1f;
    private int stepXWater;
    private int stepYWater;


    private Mesh meshGround;
    private Vector3[] verticesGround;
    private Mesh meshWater;
    private Vector3[] verticesWater;



    void Awake()
    {
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
    }

    private void Start()
    {
        if (proceduralGround == null)
            GenerateMap();
    }

    public void GenerateMap()
    {
        Cleanup();

        stepXGround = Mathf.FloorToInt(mapSize.x / (mapSize.x * groundResolution));
        stepYGround = Mathf.FloorToInt(mapSize.z / (mapSize.z * groundResolution));
        stepXWater = Mathf.FloorToInt(mapSize.x / (mapSize.x * waterResolution));
        stepYWater = Mathf.FloorToInt(mapSize.z / (mapSize.z * waterResolution));

        if (randomSeed)
            seed = Random.Range(0, 100000);

        float[,] noiseMap = Noise.GenerateNoiseMap(((int)mapSize.x * mapResolution), ((int)mapSize.z * mapResolution), seed, noiseScale, octaves, persistence, lacunarity, offset);

        ScaleUnderwaterDust();

        if (generateGras)
            PlantPlants(noiseMap);

        if (generateWalls)
            GenerateWalls();

        if (generateStones)
            PlaceStones(noiseMap);

        if (generateGround)
            GenerateGround(noiseMap);

        if (generateWater)
            GenerateWater();

        if (generateBricks)
            PlaceBricks(noiseMap);
    }


    public void PlantPlants(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int y = paddingToMapBorder; y < height - paddingToMapBorder; y++)
        {
            for (int x = paddingToMapBorder; x < width - paddingToMapBorder; x++)
            {
                float sample = noiseMap[x, y];
                float heightOffset = sample * heightScale;

                if (sample < thresholdGrass && Random.value < growDensityGrass)
                {
                    GameObject go1 = Instantiate(prefabGrass, new Vector3(0, 0, 0), Quaternion.identity);
                    float gs1 = Map.map(sample, 0f, thresholdGrass, 1, 0.1f) * grassScale;
                    go1.transform.localScale = new Vector3(gs1, gs1, gs1);
                    go1.transform.position = new Vector3((x / (float)mapResolution), heightOffset, (y / (float)mapResolution));
                    go1.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    go1.transform.parent = enviromentHolder;
                    go1.tag = "Enviroment";

                    // add gras as spawnpoint
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        if (sample < 0.3f)
                            ecoSystemManager.AddSpawnPoint(go1.transform.position + new Vector3(0, 2, 0));
                    }

                }

                if (sample > thresholdSeaweed && Random.value < growDensitySeaweed)
                {
                    GameObject go2 = Instantiate(prefabSeaweed, new Vector3(0, 0, 0), Quaternion.identity);
                    float gs2 = sample * seaweedScale;
                    go2.transform.position = new Vector3((x / (float)mapResolution), heightOffset, (y / (float)mapResolution));
                    go2.transform.localScale = new Vector3(gs2, gs2 * 10, gs2);
                    go2.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    go2.transform.position += new Vector3(0, gs2, 0);
                    go2.transform.parent = enviromentHolder;
                    go2.tag = "Enviroment";

                    // add gras as spawnpoint
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        ecoSystemManager.AddSpawnPoint(go2.transform.position + new Vector3(0, Random.Range(2, gs2 * 6), 0));
                    }

                }
            }
        }
    }

    private void GenerateWalls()
    {
        // Wall
        Vector3 position1 = new Vector3(mapSize.x / 2, mapSize.y / 2, 0);
        GameObject wall1 = Instantiate(prefabWall, position1, Quaternion.identity);
        wall1.transform.localScale = new Vector3(mapSize.x + 1, mapSize.y, 1);
        wall1.transform.parent = enviromentHolder;
        wall1.tag = "Enviroment";

        // Wall
        Vector3 position2 = new Vector3(mapSize.x / 2, mapSize.y / 2, mapSize.z);
        GameObject wall2 = Instantiate(prefabWall, position2, Quaternion.identity);
        wall2.transform.localScale = new Vector3(mapSize.x + 1, mapSize.y, 1);
        wall2.transform.parent = enviromentHolder;
        wall2.tag = "Enviroment";

        // Wall
        Vector3 position3 = new Vector3(0, mapSize.y / 2, mapSize.z / 2);
        GameObject wall3 = Instantiate(prefabWall, position3, Quaternion.identity);
        wall3.transform.localScale = new Vector3(mapSize.z + 1, mapSize.y, 1);
        wall3.transform.localEulerAngles = new Vector3(0, 90, 0);
        wall3.transform.parent = enviromentHolder;
        wall3.tag = "Enviroment";

        // Wall
        Vector3 position4 = new Vector3(mapSize.x, mapSize.y / 2, mapSize.z / 2);
        GameObject wall4 = Instantiate(prefabWall, position4, Quaternion.identity);
        wall4.transform.localScale = new Vector3(mapSize.z + 1, mapSize.y, 1);
        wall4.transform.localEulerAngles = new Vector3(0, 90, 0);
        wall4.transform.parent = enviromentHolder;
        wall4.tag = "Enviroment";

        //Bottom - Ground is the Wall
        Vector3 position5 = new Vector3(mapSize.x / 2, 0.5f, mapSize.z / 2);
        GameObject wall5 = Instantiate(prefabWall, position5, Quaternion.identity);
        wall5.transform.localScale = new Vector3(mapSize.x + 1, 1, mapSize.z + 1);
        wall5.GetComponent<MeshRenderer>().enabled = false;
        wall5.transform.parent = enviromentHolder;
        wall5.tag = "Enviroment";

        // Top
        Vector3 position6 = new Vector3(mapSize.x / 2, mapSize.y - 0.5f, mapSize.z / 2);
        GameObject wall6 = Instantiate(prefabWall, position6, Quaternion.identity);
        wall6.transform.localScale = new Vector3(mapSize.x + 1, 1, mapSize.z + 1);
        wall6.GetComponent<MeshRenderer>().enabled = false;
        wall6.transform.parent = enviromentHolder;
        wall6.tag = "Enviroment";
    }


    private void PlaceBricks(float[,] noiseMap)
    {
        for (int i = 0; i < amountSzeneElements; i++)
        {
            float elementHeight = Random.Range(3f * sizeVariationBrick, 9f * sizeVariationBrick);
            float elementWidth = Random.Range(1f * sizeVariationBrick, 3f * sizeVariationBrick);

            int x = (int)Random.Range(paddingToMapBorder, mapSize.x - paddingToMapBorder);
            int y = (int)Random.Range(paddingToMapBorder, mapSize.z - paddingToMapBorder);

            float sample = noiseMap[x, y];
            float heightOffset = sample * heightScale;

            Vector3 position = new Vector3(x, elementHeight / 2f, y);
            GameObject instName = prefabCube; //(Random.Range(0, 2) == 0) ? prefabCube : prefabCylinder;

            GameObject go = Instantiate(instName, position, Quaternion.identity);
            go.transform.localScale = new Vector3(elementWidth, elementHeight, elementWidth);
            go.transform.localEulerAngles = new Vector3(0, Random.Range(0, 90), 0);
            go.transform.parent = enviromentHolder;
            go.tag = "Enviroment";
        }
    }

    private void PlaceStones(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int y = paddingToMapBorder; y < height - paddingToMapBorder; y++)
        {
            for (int x = paddingToMapBorder; x < width - paddingToMapBorder; x++)
            {
                float sample = noiseMap[x, y];
                float heightOffset = sample * heightScale;

                if (sample > thresholdGrass + groupingStones && sample < thresholdSeaweed + groupingStones && Random.value < placingDensityStones)
                {
                    Vector3 position = new Vector3(x, heightOffset, y);
                    GameObject goTmp = stones[(int)Random.Range(0, stones.Length)];
                    GameObject rgo = Instantiate(goTmp, position, Quaternion.identity);
                    rgo.transform.localScale = new Vector3(1, 1, 1) * Random.Range(1, sizeVariation);
                    rgo.transform.eulerAngles = new Vector3(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180));
                    // Material material = rgo.GetComponent<MeshRenderer>().material;
                    // Color color = new Color();
                    //Color color = Color.Lerp(Color.black, Color.white, Random.Range(0.2f, 0.8f));
                    //rgo.GetComponent<Renderer>().material.SetColor("_BaseColor", color);

                    rgo.transform.parent = enviromentHolder;
                    rgo.tag = "Enviroment";
                }
            }
        }
    }

    private void ScaleUnderwaterDust()
    {
        underwaterDust.transform.localScale = new Vector3(mapSize.x, mapSize.z, mapSize.y);
        underwaterDust.transform.position = new Vector3(mapSize.x / 2, mapSize.y / 2, mapSize.z / 2);
    }

    private GameObject proceduralGround;

    private void GenerateGround(float[,] noiseMap)
    {
        if (proceduralGround == null)
        {
            proceduralGround = new GameObject("ProceduralGround");
            proceduralGround.tag = "Enviroment";
            proceduralGround.AddComponent<MeshRenderer>();
            proceduralGround.GetComponent<Renderer>().material = groundMaterial;
            proceduralGround.AddComponent<MeshFilter>();
            proceduralGround.GetComponent<MeshFilter>().mesh = meshGround = new Mesh();
            proceduralGround.AddComponent<MeshCollider>();
            proceduralGround.GetComponent<MeshCollider>().enabled = false;
            proceduralGround.layer = 11;
            meshGround.name = "ProceduralGround";
        }

        verticesGround = new Vector3[((int)mapSize.x + 1) * ((int)mapSize.z + 1)];
        Vector2[] uv = new Vector2[verticesGround.Length];
        Color[] color = new Color[verticesGround.Length];

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        minHeight = float.MaxValue;
        maxHeight = float.MinValue;

        for (int i = 0, y = 0; y <= (int)mapSize.z; y += stepYGround)
        {
            for (int x = 0; x <= (int)mapSize.x; x += stepXGround, i++)
            {
                int xMax = Mathf.Clamp(x, 0, width - 1);
                int yMax = Mathf.Clamp(y, 0, height - 1);
                float heightOffset = noiseMap[xMax, yMax] * heightScale;
                verticesGround[i] = new Vector3(x, heightOffset, y);
                uv[i] = new Vector2((float)x / (float)mapSize.x, (float)y / (float)mapSize.z);

                if (heightOffset > maxHeight)
                {
                    maxHeight = heightOffset;
                }
                if (heightOffset < minHeight)
                {
                    minHeight = heightOffset;
                }
            }
        }


        for (int j = 0, y = 0; y <= (int)mapSize.z; y += stepYGround)
        {
            for (int x = 0; x <= (int)mapSize.x; x += stepXGround, j++)
            {
                int xMax = Mathf.Clamp(x, 0, width - 1);
                int yMax = Mathf.Clamp(y, 0, height - 1);
                float heightOffset = verticesGround[j].y;
                color[j] = gradient.Evaluate(Mathf.InverseLerp(0, 15, heightOffset));
            }
        }

        meshGround.Clear();
        meshGround.vertices = verticesGround;
        meshGround.uv = uv;
        meshGround.colors = color;

        int[] triangles = new int[(int)mapSize.x * (int)mapSize.z * 6];
        for (int ti = 0, vi = 0, y = 0; y < (int)mapSize.z; y += stepYGround, vi++)
        {
            for (int x = 0; x < (int)mapSize.x; x += stepXGround, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + (int)mapSize.x / stepXGround + 1;
                triangles[ti + 5] = vi + (int)mapSize.x / stepXGround + 2;
            }
        }
        meshGround.triangles = triangles;
        meshGround.RecalculateNormals();
        proceduralGround.GetComponent<MeshCollider>().enabled = true;
    }

    private GameObject proceduralWater;
    private void GenerateWater()
    {
        if (proceduralWater == null)
        {
            proceduralWater = new GameObject("ProceduralWater");
            proceduralWater.tag = "Enviroment";
            proceduralWater.AddComponent<MeshRenderer>();
            proceduralWater.GetComponent<Renderer>().material = waterMaterial;
            proceduralWater.AddComponent<MeshFilter>();
            proceduralWater.GetComponent<MeshFilter>().mesh = meshWater = new Mesh();
            proceduralWater.layer = 4;
        }

        meshWater.name = "ProceduralWater";

        verticesWater = new Vector3[((int)mapSize.x + 1) * ((int)mapSize.z + 1)];
        Vector2[] uv = new Vector2[verticesWater.Length];

        for (int i = 0, y = 0; y <= (int)mapSize.z; y += stepYWater)
        {
            for (int x = 0; x <= (int)mapSize.x; x += stepXWater, i++)
            {
                verticesWater[i] = new Vector3(x, 0, y);
                uv[i] = new Vector2(x / mapSize.x, y / mapSize.z);
            }
        }

        meshWater.Clear();
        meshWater.vertices = verticesWater;
        meshWater.uv = uv;

        int[] triangles = new int[(int)mapSize.x * (int)mapSize.z * 6];
        for (int ti = 0, vi = 0, y = 0; y < (int)mapSize.z; y += stepYWater, vi++)
        {
            for (int x = 0; x < (int)mapSize.x; x += stepXWater, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + (int)mapSize.x / stepXWater + 1;
                triangles[ti + 5] = vi + (int)mapSize.x / stepXWater + 2;
            }
        }
        meshWater.triangles = triangles;
        meshWater.RecalculateNormals();

        proceduralWater.transform.Rotate(180, 0, 0, Space.Self);
        proceduralWater.transform.position = new Vector3(0, mapSize.y + 3, mapSize.z);
    }


    private void OnDrawGizmos()
    {
        if (debug)
        {
            if (verticesGround == null)
            {
                return;
            }

            Gizmos.color = Color.black;
            for (int i = 0; i < verticesGround.Length; i++)
            {
                Gizmos.DrawSphere(verticesGround[i], 0.1f);
            }
        }
    }

    public void Cleanup()
    {
        {
            GameObject[] go = GameObject.FindGameObjectsWithTag("Enviroment");
            foreach (GameObject tgo in go)
            {
                DestroyImmediate(tgo);
            }

            if (!enviromentHolder)
            {
                enviromentHolder = new GameObject("Enviroment").transform;
                enviromentHolder.tag = "Enviroment";
            }
        }
    }


    void OnValidate()
    {
        if (mapSize.x < 3)
        {
            mapSize.x = 3;
        }
        if (mapSize.y < 3)
        {
            mapSize.y = 3;
        }
        if (mapSize.z < 3)
        {
            mapSize.z = 3;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (mapResolution < 1)
        {
            mapResolution = 1;
        }
        if (paddingToMapBorder < 0)
        {
            paddingToMapBorder = 0;
        }
        if (heightScale < 0)
        {
            heightScale = 0;
        }
        if (waterResolution > 1)
        {
            waterResolution = 1;
        }
        if (waterResolution < 0.1f)
        {
            waterResolution = 0.1f;
        }
        if (groundResolution > 1)
        {
            groundResolution = 1;
        }
        if (groundResolution < 0.1f)
        {
            groundResolution = 0.1f;
        }
        if (mapResolution > 10)
        {
            mapResolution = 10;
        }
        if (mapResolution < 0)
        {
            mapResolution = 0;
        }
    }
}