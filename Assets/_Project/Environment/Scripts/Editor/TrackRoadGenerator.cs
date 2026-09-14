using UnityEngine;
using UnityEditor;

/// <summary>
/// A6 helper: builds a closed loop of stretched-cube road segments connecting
/// the waypoints under Track/Path (see TrackWaypointGenerator, A5).
/// Safe to re-run — it repositions/rescales existing segments instead of duplicating.
/// Delete this script once the real road mesh (Phase C, splines) exists.
/// </summary>
public static class TrackRoadGenerator
{
    const float RoadWidth = 14f;
    const float RoadThickness = 0.3f;
    const float RoadHeight = 0.15f; // top surface sits at y = 0.15 per BUILD_PLAN A6
    const float JointLength = 4f;   // just enough depth to bridge the corner wedge gap

    [MenuItem("DerbyDummies/Track/Generate Road From Waypoints (A6)")]
    public static void GenerateRoad()
    {
        Transform trackGO = GameObject.Find("Track")?.transform;
        Transform path = trackGO != null ? trackGO.Find("Path") : null;
        if (path == null || path.childCount < 2)
        {
            Debug.LogError("[TrackRoadGenerator] No Track/Path with waypoints found. " +
                            "Run 'Generate Oval Waypoints (A5)' first.");
            return;
        }

        Transform road = GetOrCreateChild(trackGO, "Road");
        Material roadMat = FindMaterial("M_Greybox_Road");
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
            Debug.LogWarning("[TrackRoadGenerator] 'Ground' layer not found — " +
                              "add it in Project Settings > Tags and Layers. Segments left on Default layer for now.");

        int count = path.childCount;
        for (int i = 0; i < count; i++)
        {
            Vector3 a = path.GetChild(i).position;
            Vector3 b = path.GetChild((i + 1) % count).position;
            Vector3 mid = (a + b) * 0.5f + Vector3.up * RoadHeight;
            float length = Vector3.Distance(a, b);
            Quaternion rot = Quaternion.LookRotation((b - a).normalized, Vector3.up);

            string name = $"Seg_{i:D2}";
            Transform seg = road.Find(name);
            GameObject segGO;
            if (seg == null)
            {
                segGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segGO.name = name;
                Undo.RegisterCreatedObjectUndo(segGO, "Create Road Segment");
                segGO.transform.SetParent(road, false);
            }
            else
            {
                segGO = seg.gameObject;
            }

            segGO.transform.position = mid;
            segGO.transform.rotation = rot;
            segGO.transform.localScale = new Vector3(RoadWidth, RoadThickness, length);

            if (groundLayer >= 0) segGO.layer = groundLayer;

            if (roadMat != null)
                segGO.GetComponent<MeshRenderer>().sharedMaterial = roadMat;
        }

        // Corners: adjacent segments point in different directions, so their flat
        // end-caps don't line up and leave a wedge-shaped gap on the outside of the
        // turn. Bridge every waypoint with a square joint block big enough to cover it.
        for (int i = 0; i < count; i++)
        {
            Vector3 prev = path.GetChild((i - 1 + count) % count).position;
            Vector3 here = path.GetChild(i).position;
            Vector3 next = path.GetChild((i + 1) % count).position;

            Vector3 dirIn = (here - prev).normalized;
            Vector3 dirOut = (next - here).normalized;
            Vector3 avgDir = (dirIn + dirOut).sqrMagnitude > 0.0001f
                ? (dirIn + dirOut).normalized
                : dirIn;

            string jointName = $"Joint_{i:D2}";
            Transform joint = road.Find(jointName);
            GameObject jointGO;
            if (joint == null)
            {
                jointGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jointGO.name = jointName;
                Undo.RegisterCreatedObjectUndo(jointGO, "Create Road Joint");
                jointGO.transform.SetParent(road, false);
            }
            else
            {
                jointGO = joint.gameObject;
            }

            jointGO.transform.position = here + Vector3.up * RoadHeight;
            jointGO.transform.rotation = Quaternion.LookRotation(avgDir, Vector3.up);
            jointGO.transform.localScale = new Vector3(RoadWidth, RoadThickness, JointLength);

            if (groundLayer >= 0) jointGO.layer = groundLayer;
            if (roadMat != null)
                jointGO.GetComponent<MeshRenderer>().sharedMaterial = roadMat;
        }

        Selection.activeGameObject = road.gameObject;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[TrackRoadGenerator] Built {count} road segments under Track/Road." +
                   (roadMat == null ? " (M_Greybox_Road material not found — segments left default grey)" : ""));
    }

    static Material FindMaterial(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"{name} t:Material");
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing;

        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}
