using UnityEngine;
using UnityEditor;

/// <summary>
/// A7 helper: builds inner + outer edge walls following the road segments under
/// Track/Path (see TrackWaypointGenerator A5, TrackRoadGenerator A6).
/// Safe to re-run — repositions existing wall pieces instead of duplicating.
/// Delete once the real spline-based road + walls (Phase C) exist.
/// </summary>
public static class TrackWallGenerator
{
    const float RoadWidth = 14f;
    const float WallThickness = 0.5f;
    const float WallHeight = 2f;
    const float WallCenterY = WallHeight / 2f;
    const float JointLength = 4f; // matches the road's corner-joint margin

    [MenuItem("DerbyDummies/Track/Generate Walls (A7)")]
    public static void GenerateWalls()
    {
        Transform trackGO = GameObject.Find("Track")?.transform;
        Transform path = trackGO != null ? trackGO.Find("Path") : null;
        if (path == null || path.childCount < 2)
        {
            Debug.LogError("[TrackWallGenerator] No Track/Path with waypoints found. Run A5 first.");
            return;
        }

        Transform walls = GetOrCreateChild(trackGO, "Walls");
        Material wallMat = FindMaterial("M_Greybox_Wall");
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer < 0)
            Debug.LogWarning("[TrackWallGenerator] 'Wall' layer not found — add it in Project Settings > Tags and Layers.");

        float halfWidth = RoadWidth / 2f;
        int count = path.childCount;

        for (int i = 0; i < count; i++)
        {
            Vector3 a = path.GetChild(i).position;
            Vector3 b = path.GetChild((i + 1) % count).position;
            Vector3 tangent = (b - a).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            float length = Vector3.Distance(a, b);
            Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up);
            Vector3 mid = (a + b) * 0.5f;

            Vector3 outerDir = OuterSide(mid, right, halfWidth);

            BuildWallSegment(walls, $"Outer_{i:D2}", mid + outerDir * halfWidth, rot, length, wallMat, wallLayer);
            BuildWallSegment(walls, $"Inner_{i:D2}", mid - outerDir * halfWidth, rot, length, wallMat, wallLayer);
        }

        // Corner joints at every waypoint, both sides — bridges the same wedge gap the road has.
        for (int i = 0; i < count; i++)
        {
            Vector3 prev = path.GetChild((i - 1 + count) % count).position;
            Vector3 here = path.GetChild(i).position;
            Vector3 next = path.GetChild((i + 1) % count).position;

            Vector3 dirIn = (here - prev).normalized;
            Vector3 dirOut = (next - here).normalized;
            Vector3 avgDir = (dirIn + dirOut).sqrMagnitude > 0.0001f ? (dirIn + dirOut).normalized : dirIn;
            Vector3 right = Vector3.Cross(Vector3.up, avgDir).normalized;
            Quaternion rot = Quaternion.LookRotation(avgDir, Vector3.up);

            Vector3 outerDir = OuterSide(here, right, halfWidth);

            BuildWallSegment(walls, $"OuterJoint_{i:D2}", here + outerDir * halfWidth, rot, JointLength, wallMat, wallLayer);
            BuildWallSegment(walls, $"InnerJoint_{i:D2}", here - outerDir * halfWidth, rot, JointLength, wallMat, wallLayer);
        }

        Selection.activeGameObject = walls.gameObject;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[TrackWallGenerator] Built {count * 2} wall segments + {count * 2} corner joints under Track/Walls.");
    }

    // For a convex loop, whichever side lands farther from the track's origin-centered
    // interior is the "outer" edge — avoids needing a fixed winding-direction assumption.
    static Vector3 OuterSide(Vector3 point, Vector3 right, float halfWidth)
    {
        Vector3 plus = point + right * halfWidth;
        Vector3 minus = point - right * halfWidth;
        return plus.sqrMagnitude >= minus.sqrMagnitude ? right : -right;
    }

    static void BuildWallSegment(Transform parent, string name, Vector3 pos, Quaternion rot, float length, Material mat, int layer)
    {
        Transform existing = parent.Find(name);
        GameObject go;
        if (existing == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = existing.gameObject;
        }

        go.transform.position = new Vector3(pos.x, WallCenterY, pos.z);
        go.transform.rotation = rot;
        go.transform.localScale = new Vector3(WallThickness, WallHeight, length);

        if (layer >= 0) go.layer = layer;
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
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
