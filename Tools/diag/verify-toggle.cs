// Sanity check: do the _Caustics / _Ripple toggles actually change the rendered image?
// If the direct image difference is ~0, the previous A/B was meaningless.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
const int W = 800, H = 450;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var porcelain = new System.Collections.Generic.List<UnityEngine.Material>();
var water = new System.Collections.Generic.List<UnityEngine.Material>();
var seen = new System.Collections.Generic.HashSet<UnityEngine.Material>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (!m || seen.Contains(m)) continue;
    seen.Add(m);
    if (m.shader.name == "Poolcore/Porcelain") porcelain.Add(m);
    else if (m.shader.name == "Poolcore/Still Water") water.Add(m);
}
var c0 = new System.Collections.Generic.List<float>();
var r0 = new System.Collections.Generic.List<float>();
foreach (var m in porcelain) c0.Add(m.GetFloat("_Caustics"));
foreach (var m in water) r0.Add(m.GetFloat("_Ripple"));
sb.AppendLine("porcelain=" + porcelain.Count + " water=" + water.Count);
foreach (var m in porcelain) sb.AppendLine("  porcelain " + m.name + " _Caustics=" + m.GetFloat("_Caustics"));
foreach (var m in water) sb.AppendLine("  water " + m.name + " _Ripple=" + m.GetFloat("_Ripple"));

System.Func<UnityEngine.Color32[]> snap = delegate
{
    var rt = new UnityEngine.RenderTexture(W, H, 24);
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    try
    {
        cam.targetTexture = rt; cam.Render();
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
        t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
        UnityEngine.RenderTexture.active = prev;
        return t.GetPixels32();
    }
    finally { cam.targetTexture = null; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(t); }
};
System.Func<UnityEngine.Color32[], UnityEngine.Color32[], string> cmp = delegate (UnityEngine.Color32[] a, UnityEngine.Color32[] b)
{
    long sum = 0; int maxd = 0, over8 = 0;
    for (int i = 0; i < a.Length; i++)
    {
        int m = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - b[i].r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - b[i].g), UnityEngine.Mathf.Abs(a[i].b - b[i].b)));
        sum += m; if (m > maxd) maxd = m; if (m > 8) over8++;
    }
    return "mean=" + ((double)sum / a.Length).ToString("F3") + "  max=" + maxd + "  pixels>8=" + over8;
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    cam.transform.position = new UnityEngine.Vector3(0f, 1.5f, -5.5f);
    cam.transform.LookAt(new UnityEngine.Vector3(0f, -0.1f, 6f));
    var baseFrame = snap();
    foreach (var m in porcelain) m.SetFloat("_Caustics", 0f);
    sb.AppendLine("caustics OFF vs base : " + cmp(baseFrame, snap()));
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", c0[k]);
    foreach (var m in water) m.SetFloat("_Ripple", 0f);
    sb.AppendLine("ripple   OFF vs base : " + cmp(baseFrame, snap()));
    for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", r0[k]);
    // also: hide the water renderers entirely, to see how much of the frame is water
    var wr = new System.Collections.Generic.List<UnityEngine.Renderer>();
    foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
        if (r.sharedMaterial && r.sharedMaterial.shader.name == "Poolcore/Still Water") wr.Add(r);
    foreach (var r in wr) r.enabled = false;
    sb.AppendLine("water hidden vs base : " + cmp(baseFrame, snap()));
    foreach (var r in wr) r.enabled = true;
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", c0[k]);
    for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", r0[k]);
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
}
return sb.ToString();
