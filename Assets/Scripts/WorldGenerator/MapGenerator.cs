using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGenerator : MonoBehaviour
{

    EcoSystemManager ecoSystemManager;

    [Header("Stones")]
    public GameObject prefabCube;
    public GameObject prefabCylinder;
    public int amountSzeneElements;


    [Header("Grass")]
    public GameObject prefabGrass;

    [Range(0, 0.5f)]
    public float thresholdGrass;
    public float grassScale;


    [Header("Seaweed")]
    public GameObject prefabSeaweed;
    [Range(0.5f, 1)]
    public float thresholdSeaweed;
    public float seaweedScale;


    [Header("General World Settings")]
    public GameObject prefabWall;
    public int paddingToMapBorder;
    public bool autoUpdate;
    Transform enviromentHolder;
    public int mapResolution;
    public Vector3 mapSize;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;
    public int seed;
    public bool randomSeed;
    public Vector2 offset;

    [Header("Ground Setting")]
    public bool debug = false;
    [Range(0, 20f)]
    public float heightScale;
    public Material groundMaterial;





    private Mesh mesh;
    private Vector3[] verticesGround;




    void Awake()
    {
        verticesGround = new Vector3[((int)mapSize.x + 1) * ((int)mapSize.z + 1)];
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
    }

    private void Start()
    {
        Cleanup();
        GenerateMap();
    }

    public void GenerateMap()
    {
        Cleanup();

        if (randomSeed)
            seed = Random.Range(0, 100000);

        float[,] noiseMap = Noise.GenerateNoiseMap(((int)mapSize.x * mapResolution), ((int)mapSize.z * mapResolution), seed, noiseScale, octaves, persistence, lacunarity, offset);

        //MapDisplay display = FindObjectOfType<MapDisplay>();
        //display.DrawNoiseMap(noiseMap, mapResolution);

        PlantPlants(noiseMap);
        GenerateWalls();
        placeStones(noiseMap);

        GenerateGround(noiseMap);
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
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

                if (sample < thresholdGrass)
                {
                    GameObject go1 = Instantiate(prefabGrass, new Vector3(0, 0, 0), Quaternion.identity);
                    //float gs1 = sample * grassScale;

                    float gs1 = map(sample, 0f, thresholdGrass, 1, 0.1f) * grassScale;

                    go1.transform.localScale = new Vector3(gs1, gs1, gs1);
                    go1.transform.position = new Vector3((x / (float)mapResolution), heightOffset, (y / (float)mapResolution));
                    go1.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    go1.transform.parent = enviromentHolder;
                    go1.tag = "Enviroment";
                }

                if (sample > thresholdSeaweed)
                {
                    GameObject go2 = Instantiate(prefabSeaweed, new Vector3((x / (float)mapResolution), heightOffset / 2f, (y / (float)mapResolution)), Quaternion.identity);
                    float gs2 = sample * seaweedScale;
                    go2.transform.localScale = new Vector3(gs2, gs2 * 10, gs2);
                    go2.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    go2.transform.position += new Vector3(0, gs2, 0);
                    go2.transform.parent = enviromentHolder;
                    go2.tag = "Enviroment";

                    // add gras as spawnpoint
                    if (!Application.isEditor)
                        ecoSystemManager.AddSpawnPoint(go2.transform.position + new Vector3(0, 3, 0));
                }
            }
        }
    }

    private void GenerateWalls()
    {
        // Wall
        Vector3 position1 = new Vector3(mapSize.x / 2, mapSize.y / 2, 0);
        GameObject wall1 = Instantiate(prefabWall, position1, Quaternion.identity);
        wall1.transform.localScale = new Vector3(mapSize.x, mapSize.y, 1);
        wall1.transform.parent = enviromentHolder;
        wall1.tag = "Enviroment";

        // Wall
        Vector3 position2 = new Vector3(mapSize.x / 2, mapSize.y / 2, mapSize.z);
        GameObject wall2 = Instantiate(prefabWall, position2, Quaternion.identity);
        wall2.transform.localScale = new Vector3(mapSize.x, mapSize.y, 1);
        wall2.transform.parent = enviromentHolder;
        wall2.tag = "Enviroment";

        // Wall
        Vector3 position3 = new Vector3(0, mapSize.y / 2, mapSize.z / 2);
        GameObject wall3 = Instantiate(prefabWall, position3, Quaternion.identity);
        wall3.transform.localScale = new Vector3(mapSize.z, mapSize.y, 1);
        wall3.transform.localEulerAngles = new Vector3(0, 90, 0);
        wall3.transform.parent = enviromentHolder;
        wall3.tag = "Enviroment";

        // Wall
        Vector3 position4 = new Vector3(mapSize.x, mapSize.y / 2, mapSize.z / 2);
        GameObject wall4 = Instantiate(prefabWall, position4, Quaternion.identity);
        wall4.transform.localScale = new Vector3(mapSize.z, mapSize.y, 1);
        wall4.transform.localEulerAngles = new Vector3(0, 90, 0);
        wall4.transform.parent = enviromentHolder;
        wall4.tag = "Enviroment";

        // Bottom
        Vector3 position5 = new Vector3(mapSize.x / 2, 1, mapSize.z / 2);
        GameObject wall5 = Instantiate(prefabWall, position5, Quaternion.identity);
        wall5.transform.localScale = new Vector3(mapSize.x, 1, mapSize.z);
        wall5.GetComponent<MeshRenderer>().enabled = false;
        wall5.transform.parent = enviromentHolder;
        wall5.tag = "Enviroment";

        // Top
        Vector3 position6 = new Vector3(mapSize.x / 2, mapSize.y - 1, mapSize.z / 2);
        GameObject wall6 = Instantiate(prefabWall, position6, Quaternion.identity);
        wall6.transform.localScale = new Vector3(mapSize.x, 1, mapSize.z);
        wall6.GetComponent<MeshRenderer>().enabled = false;
        wall6.transform.parent = enviromentHolder;
        wall6.tag = "Enviroment";
    }


    private void placeStones(float[,] noiseMap)
    {
        for (int i = 0; i < amountSzeneElements; i++)
        {
            float elementHeight = Random.Range(4f, 20f);
            float elementWidth = Random.Range(3f, 8f);

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

    private void GenerateGround(float[,] noiseMap)
    {
        GameObject proceduralGround = new GameObject("ProceduralGround");
        proceduralGround.tag = "Enviroment";
        proceduralGround.AddComponent<MeshRenderer>();
        proceduralGround.GetComponent<Renderer>().material = groundMaterial;
        proceduralGround.AddComponent<MeshFilter>();
        proceduralGround.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "ProceduralGround";

        verticesGround = new Vector3[((int)mapSize.x + 1) * ((int)mapSize.z + 1)];
        Vector2[] uv = new Vector2[verticesGround.Length];

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int i = 0, y = 0; y <= (int)mapSize.z; y++)
        {
            for (int x = 0; x <= mapSize.x; x++, i++)
            {
                int xMax = Mathf.Clamp(x, 0, width - 1);
                int yMax = Mathf.Clamp(y, 0, height - 1);
                float heightOffset = noiseMap[xMax, yMax] * heightScale;
                verticesGround[i] = new Vector3(x, heightOffset, y);
                uv[i] = new Vector2(x / mapSize.x, y / mapSize.z);
            }
        }
        mesh.vertices = verticesGround;
        mesh.uv = uv;

        int[] triangles = new int[(int)mapSize.x * (int)mapSize.z * 6];
        for (int ti = 0, vi = 0, y = 0; y < (int)mapSize.z; y++, vi++)
        {
            for (int x = 0; x < mapSize.x; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + (int)mapSize.x + 1;
                triangles[ti + 5] = vi + (int)mapSize.x + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
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
        if (mapSize.x < 10)
        {
            mapSize.x = 10;
        }
        if (mapSize.y < 10)
        {
            mapSize.y = 10;
        }
        if (mapSize.z < 10)
        {
            mapSize.z = 10;
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
    }

}
