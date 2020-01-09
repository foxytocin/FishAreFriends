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
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color color1;
    public Color color2;
    public bool debug = false;
    public GizmoType showSpawnRegion;
    Transform fishHolder;


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


    private IEnumerator InitializeBoidSlowly() {
        
        int count = 0;
        while (count < spawnCount)        
        {

                Vector3 pos = Random.insideUnitSphere * spawnRadius;
                Boid boid = Instantiate(prefab, pos, Quaternion.identity);
                boid.transform.parent = fishHolder;
                int cellIndex = cellGroups.GetIndex(transform.position);
                boid.cellIndex = cellIndex;
                cellGroups.RegisterAtCell(boid);

                boid.PassColor(color1, color2);
                boid.Initialize(settings, null);
                boid.RespawnBoid();

                //Debug.Log("Initializing Boid #:" +count);
                yield return new WaitForSeconds(0.025f);

                count++;
        }

        //boidManager.BoidInitializationCompleted();
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