using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    CellGroups cellGroups;
    EcoSystemManager ecoSystemManager;
    BoidManager boidManager;
    public BoidSettings settings;
    public enum GizmoType { Never, SelectedOnly, Always }
    public bool spawnBoids = true;
    public Boid prefab;
    public GameObject opponentPlayerPrefab;
    public GameObject predatorPrefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color color1;
    public Color color2;
    public bool debug = false;
    public GizmoType showSpawnRegion;
    Transform fishHolder;

    private Color[] opponentPlayerColor1 = { Color.cyan, Color.yellow, Color.blue };
    private Color[] opponentPlayerColor2 = { Color.cyan, Color.yellow, Color.blue };

    void Awake()
    {
        cellGroups = FindObjectOfType<CellGroups>();
        ecoSystemManager = FindObjectOfType<EcoSystemManager>();
        boidManager = FindObjectOfType<BoidManager>();
        fishHolder = new GameObject("Fishes").transform;
    }

    void Start()
    {
        StartCoroutine(InitializeBoidSlowly());
    }

    public void SpawnOpponentPlayers()
    {
        for (int i = 0; i < 3; i++)
        {
            MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
            Vector3 pos = new Vector3(
                Random.Range(3f, mapGenerator.mapSize.x - 3f),
                Random.Range(mapGenerator.heightScale + 1f, mapGenerator.mapSize.y - 5f),
                Random.Range(3f, mapGenerator.mapSize.z - 3f)
            );
            GameObject tempGameObject = Instantiate(opponentPlayerPrefab, pos, Quaternion.identity);
            tempGameObject.GetComponent<Leader>().leaderColor1 = opponentPlayerColor1[i];
            tempGameObject.GetComponent<Leader>().leaderColor2 = opponentPlayerColor2[i];
        }
    }

    public void SpawnPredators()
    {
        for (int i = 0; i < 1; i++)
        {
            MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
            Vector3 pos = new Vector3(
                Random.Range(3f, mapGenerator.mapSize.x - 3f),
                Random.Range(mapGenerator.heightScale + 1f, mapGenerator.mapSize.y - 5f),
                Random.Range(3f, mapGenerator.mapSize.z - 3f)
            );
            Instantiate(predatorPrefab, pos, Quaternion.identity);

        }
    }


    private IEnumerator InitializeBoidSlowly()
    {

        int count = 0;

        yield return new WaitForSeconds(10);

        while (count < spawnCount)
        {

            Vector3 pos = Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate(prefab, pos, Quaternion.identity);
            boid.transform.parent = fishHolder;

            boid.PassColor(color1, color2);
            boid.Initialize(settings, null);
            boid.RespawnBoid();

            //Debug.Log("Initializing Boid #:" +count);
            yield return new WaitForSeconds(0.015f); //0.25

            count++;
        }

        boidManager.BoidInitializationCompleted();
    }


    private void OnDrawGizmos()
    {
        if (debug && showSpawnRegion == GizmoType.Always)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (debug && showSpawnRegion == GizmoType.SelectedOnly)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = new Color(color1.r, color1.g, color1.b, 0.3f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }
    }

}