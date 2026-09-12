using UnityEngine;

/// <summary>
/// Marker for a grid start slot. The environment owner places and orders these;
/// the kart / race code reads them to position racers at the start.
/// Field names are a cross-team contract — coordinate before renaming.
/// </summary>
[AddComponentMenu("DerbyDummies/Track/Spawn Point")]
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Grid order. 1 = pole position.")]
    public int gridIndex = 1;

    static readonly Vector3 KartSize = new Vector3(1.2f, 1f, 2.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        // Arrow = the direction the kart should face.
        Gizmos.DrawLine(transform.position,
                        transform.position + transform.forward * 3f);

        Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * 0.5f,
                                      transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, KartSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
