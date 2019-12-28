using UnityEngine;

public class Spawner : MonoBehaviour
{

    public enum GizmoType { Never, SelectedOnly, Always }
    public bool spawnBoids = true;
    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color color1;
    public Color color2;
    public bool debug = false;
    public GizmoType showSpawnRegion;

    void Start()
    {
        var fishHolder = new GameObject("Fishes").transform;

        if (spawnBoids)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
                Boid boid = Instantiate(prefab);
                boid.transform.parent = fishHolder;
                boid.PassColor(color1, color2);
                boid.RespawnBoid();
            }
        }
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