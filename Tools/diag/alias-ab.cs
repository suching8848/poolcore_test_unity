// Attribution A/B: same cameras, four material configurations. Aliasing is measured as
// the mismatch between a 1x render and a 2x render box-downsampled to 1x (time-independent).
//   base        : as authored
//   noCaustics  : Porcelain._Caustics = 0   (isolates the animated pool-light filaments)
//   noRipple    : Still Water._Ripple = 0   (isolates the water surface normal/refraction)
//   neither     : both off
// All material values are restored in finally and never saved to disk.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
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
var caustics0 = new System.Collections.Generic.List<float>();
var ripple0 = new System.Collections.Generic.List<float>();
foreach (var m in porcelain) caustics0.Add(m.GetFloat("_Caustics"));
foreach (var m in water) ripple0.Add(m.GetFloat("_Ripple"));
sb.AppendLine("porcelain materials " + porcelain.Count + "  water materials " + water.Count + "  (W=" + W + " H=" + H + ")");

System.Func<int, int, UnityEngine.Color32[]> render = delegate (int w, int h)
{
    var rt = new UnityEngine.RenderTexture(w, h, 24);
    var t = new UnityEngine.Texture2D(w, h, UnityEngine.TextureFormat.RGB24, false);
    try
    {
        cam.targetTexture = rt; cam.Render();
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
        t.ReadPixels(new UnityEngine.Rect(0, 0, w, h), 0, 0); t.Apply();
        UnityEngine.RenderTexture.active = prev;
        return t.GetPixels32();
    }
    finally { cam.targetTexture = null; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(t); }
};
System.Func<int[]> measure = delegate
{
    var a = render(W, H);
    var big = render(W * 2, H * 2);
    int over12 = 0, over40 = 0, maxd = 0;
    for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            int i = y * W + x, r = 0, g = 0, b = 0;
            for (int dy = 0; dy < 2; dy++)
                for (int dx = 0; dx < 2; dx++)
                { var p = big[(y * 2 + dy) * (W * 2) + x * 2 + dx]; r += p.r; g += p.g; b += p.b; }
            int m = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - r / 4), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - g / 4), UnityEngine.Mathf.Abs(a[i].b - b / 4)));
            if (m > maxd) maxd = m;
            if (m > 12) over12++;
            if (m > 40) over40++;
        }
    return new int[] { over12, over40, maxd };
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }

    var poses = new string[] { "atrium-wide", "waterline" };
    foreach (var pose in poses)
    {
        if (pose == "atrium-wide") { cam.transform.position = new UnityEngine.Vector3(0f, 1.71f, -10f); cam.transform.rotation = UnityEngine.Quaternion.identity; }
        else { cam.transform.position = new UnityEngine.Vector3(0f, 1.5f, -5.5f); cam.transform.LookAt(new UnityEngine.Vector3(0f, -0.1f, 6f)); }
        sb.AppendLine("=== " + pose);
        for (int cfg = 0; cfg < 4; cfg++)
        {
            bool noCaustics = (cfg == 1 || cfg == 3), noRipple = (cfg == 2 || cfg == 3);
            for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", noCaustics ? 0f : caustics0[k]);
            for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", noRipple ? 0f : ripple0[k]);
            var r = measure();
            string name = cfg == 0 ? "base       " : (cfg == 1 ? "noCaustics " : (cfg == 2 ? "noRipple   " : "neither    "));
            sb.AppendLine("  " + name + "  |d|>12 = " + r[0].ToString().PadLeft(7) + "   |d|>40 = " + r[1].ToString().PadLeft(6) + "   max = " + r[2]);
        }
        for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", caustics0[k]);
        for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", ripple0[k]);
    }
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", caustics0[k]);
    for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", ripple0[k]);
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
}
sb.AppendLine("restored _Caustics=" + caustics0.Count + " values, _Ripple=" + ripple0.Count + " values");
return sb.ToString();
