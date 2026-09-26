// Detects coplanar same-side overlapping faces between axis-aligned box renderers.
// Two opaque faces lying on the same plane, facing the same way, and overlapping in
// area will z-fight: the depth test flips per pixel and per frame as the camera moves.
var sb = new System.Text.StringBuilder();
var all = UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None);
var items = new System.Collections.Generic.List<UnityEngine.MeshRenderer>();
foreach (var r in all)
{
    if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
    var e = r.transform.eulerAngles;
    bool aligned = true;
    foreach (float ang in new float[] { e.x, e.y, e.z })
    {
        float best = 999f;
        foreach (float s in new float[] { 0f, 90f, 180f, 270f })
            best = UnityEngine.Mathf.Min(best, UnityEngine.Mathf.Abs(UnityEngine.Mathf.DeltaAngle(ang, s)));
        if (best > 0.01f) aligned = false;
    }
    if (aligned) items.Add(r);
}
var findings = new System.Collections.Generic.List<string>();
for (int i = 0; i < items.Count; i++)
{
    for (int j = i + 1; j < items.Count; j++)
    {
        var ma = items[i].sharedMaterial;
        var mb = items[j].sharedMaterial;
        if (ma && ma.renderQueue >= 3000) continue;
        if (mb && mb.renderQueue >= 3000) continue;
        var a = items[i].bounds;
        var b = items[j].bounds;
        for (int axis = 0; axis < 3; axis++)
        {
            for (int side = 0; side < 2; side++)
            {
                float pa = side == 0 ? a.min[axis] : a.max[axis];
                float pb = side == 0 ? b.min[axis] : b.max[axis];
                if (UnityEngine.Mathf.Abs(pa - pb) > 5e-4f) continue;
                int u = (axis + 1) % 3, v = (axis + 2) % 3;
                float ou = UnityEngine.Mathf.Min(a.max[u], b.max[u]) - UnityEngine.Mathf.Max(a.min[u], b.min[u]);
                float ov = UnityEngine.Mathf.Min(a.max[v], b.max[v]) - UnityEngine.Mathf.Max(a.min[v], b.min[v]);
                if (ou <= 1e-3f || ov <= 1e-3f) continue;
                string ax = axis == 0 ? "X" : (axis == 1 ? "Y" : "Z");
                findings.Add(string.Format("{0}-{1} @ {2,8:F4}  area {3,8:F4} m2  [{4} / {5}] <-> [{6} / {7}]",
                    ax, side == 0 ? "min" : "max", pa, ou * ov,
                    items[i].name, ma ? ma.name : "?",
                    items[j].name, mb ? mb.name : "?"));
            }
        }
    }
}
sb.AppendLine("axis-aligned opaque renderers: " + items.Count + " of " + all.Length);
sb.AppendLine("COPLANAR OVERLAPPING PAIRS: " + findings.Count);
for (int k = 0; k < findings.Count && k < 40; k++) sb.AppendLine("  " + findings[k]);
if (findings.Count > 40) sb.AppendLine("  ... " + (findings.Count - 40) + " more");
return sb.ToString();
