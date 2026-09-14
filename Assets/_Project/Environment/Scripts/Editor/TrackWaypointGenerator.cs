using UnityEngine;
using UnityEditor;

/// <summary>
/// A5 helper: lays out an oval ring of waypoint empties (WP_00, WP_01, ...)
/// under Track/Path, evenly spaced around an ellipse (~50m apart).
/// Safe to re-run — it repositions existing waypoints instead of duplicating them.
/// Delete this script once the real track shape (Phase C, splines) exists.
/// </summary>
public static class TrackWaypointGenerator
{
    const int WaypointCount = 16;
    const float SemiMajorAxisX = 150f;
    const float SemiMinorAxisZ = 100f;

    [MenuItem("DerbyDummies/Track/Generate Oval Waypoints (A5)")]
    public static void GenerateOvalWaypoints()
    {
        Transform track = GetOrCreateChild(null, "Track");
        Transform path = GetOrCreateChild(track, "Path");

        for (int i = 0; i < WaypointCount; i++)
        {
            float angle = i * Mathf.PI * 2f / WaypointCount;
            float x = Mathf.Cos(angle) * SemiMajorAxisX;
            float z = Mathf.Sin(angle) * SemiMinorAxisZ;

            Transform wp = GetOrCreateChild(path, $"WP_{i:D2}");
            wp.localPosition = new Vector3(x, 0f, z);
        }

        Selection.activeGameObject = path.gameObject;
        EditorSceneManagerMarkDirty();
        Debug.Log($"[TrackWaypointGenerator] Placed {WaypointCount} waypoints under Track/Path " +
                  $"in a {SemiMajorAxisX * 2}m x {SemiMinorAxisZ * 2}m oval.");
    }

    static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent == null
            ? (GameObject.Find(name)?.transform)
            : parent.Find(name);

        if (existing != null) return existing;

        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        if (parent != null) go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void EditorSceneManagerMarkDirty()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
