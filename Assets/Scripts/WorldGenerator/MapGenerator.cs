using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
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
        placeStones();
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
                if (sample < thresholdGrass)
                {
                    GameObject go1 = Instantiate(prefabGrass, new Vector3((x / (float)mapResolution), 0, (y / (float)mapResolution)), Quaternion.identity);
                    float gs1 = sample * grassScale;
                    go1.transform.localScale = new Vector3(gs1, gs1, gs1);
                    go1.transform.localEulerAngles += new Vector3(0, Random.Range(0, 360), 0);
                    go1.transform.parent = enviromentHolder;
                    go1.tag = "Enviroment";
                }

                if (sample > thresholdSeaweed)
                {
                    GameObject go2 = Instantiate(prefabSeaweed, new Vector3((x / (float)mapResolution), 0, (y / (float)mapResolution)), Quaternion.identity);
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


    private void placeStones()
    {
        for (int i = 0; i < amountSzeneElements; i++)
        {
            float elementHeight = Random.Range(4f, 20f);
            float elementWidth = Random.Range(3f, 8f);

            Vector2 pos = new Vector2(Random.Range(paddingToMapBorder, mapSize.x - paddingToMapBorder), Random.Range(paddingToMapBorder, mapSize.z - paddingToMapBorder));
            Vector3 position = new Vector3(pos[0], elementHeight / 2f, pos[1]);


            List<Vector3> hitPoints;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(pos[0], 10, pos[1]), Vector3.down, out hit, 100))
            {
                Debug.Log(hit.transform.position);
                Debug.Log(hit.point);
                hitPoints = new List<Vector3>();
                hitPoints.Add(transform.position);
                //FireRay(transform.position, transform.forward);
                Gizmos.color = Color.blue;

                for (int j = 1; j < hitPoints.Count; j++)
                {
                    Gizmos.DrawLine(hitPoints[i - 1], hitPoints[i]);
                }
            }












            GameObject instName = prefabCube; //(Random.Range(0, 2) == 0) ? prefabCube : prefabCylinder;

            GameObject go = Instantiate(instName, position, Quaternion.identity);
            go.transform.localScale = new Vector3(elementWidth, elementHeight, elementWidth);
            go.transform.localEulerAngles = new Vector3(0, Random.Range(0, 90), 0);
            go.transform.parent = enviromentHolder;
            go.tag = "Enviroment";
        }
    }

    private void Cleanup()
    {

        if (!enviromentHolder)
        {
            enviromentHolder = new GameObject("Enviroment").transform;
        }
        else
        {
            GameObject[] go = GameObject.FindGameObjectsWithTag("Enviroment");

            foreach (GameObject tgo in go)
            {
                DestroyImmediate(tgo);
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
    }

}
