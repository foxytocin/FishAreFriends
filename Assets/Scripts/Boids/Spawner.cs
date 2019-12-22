using UnityEngine;

public class Spawner : MonoBehaviour
{

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color color1;
    public Color color2;
    public GizmoType showSpawnRegion;

    void Awake()
    {
        var fishHolder = new GameObject("Fishes").transform;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.parent = fishHolder;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour(color1, color2);
        }
    }


    private void OnDrawGizmos()
    {
        if (showSpawnRegion == GizmoType.Always)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showSpawnRegion == GizmoType.SelectedOnly)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {

        Gizmos.color = new Color(color1.r, color1.g, color1.b, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }

}