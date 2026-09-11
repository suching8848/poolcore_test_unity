// Motion-shimmer map: drive a synthetic walking path (12 frames, 4 cm + small yaw each),
// accumulate per-pixel min/max across frames, and output the temporal-contrast map.
// Pixels with a large range are exactly the ones that "shimmer while you move".
// A/B configs isolate the cause: water sharp specular (_Ripple), porcelain gloss
// (_Smoothness), sun shadows, and the AA/post stack.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1200, H = 675, N = 12;
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

var porcelain = new System.Collections.Generic.List<UnityEngine.Material>();
var water = new System.Collections.Generic.List<UnityEngine.Material>();
var seen = new System.Collections.Generic.HashSet<UnityEngine.Material>();
var suns = new System.Collections.Generic.List<UnityEngine.Light>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (!m || seen.Contains(m)) continue;
    seen.Add(m);
    if (m.shader.name == "Poolcore/Porcelain") porcelain.Add(m);
    else if (m.shader.name == "Poolcore/Still Water") water.Add(m);
}
foreach (var l in UnityEngine.Object.FindObjectsByType<UnityEngine.Light>())
    if (l.type == UnityEngine.LightType.Directional) suns.Add(l);
var c0 = new System.Collections.Generic.List<float>();
var r0 = new System.Collections.Generic.List<float>();
var s0 = new System.Collections.Generic.List<float>();
foreach (var m in porcelain) { c0.Add(m.GetFloat("_Caustics")); s0.Add(m.GetFloat("_Smoothness")); }
foreach (var m in water) r0.Add(m.GetFloat("_Ripple"));
var sunShadow0 = new System.Collections.Generic.List<UnityEngine.LightShadows>();
foreach (var l in suns) sunShadow0.Add(l.shadows);

var rt = new UnityEngine.RenderTexture(W, H, 24);
System.Func<UnityEngine.Color32[]> shoot = delegate
{
    cam.targetTexture = rt; cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t); return px;
};
// returns [pixels>24, pixels>64, maxRange] and optionally writes the map
System.Func<string, bool, int[]> run = delegate (string label, bool saveMap)
{
    var lo = new byte[W * H * 3];
    var hi = new byte[W * H * 3];
    for (int f = 0; f < N; f++)
    {
        // walking forward 4 cm/frame along +Z plus a 0.03 deg yaw wiggle
        cam.transform.position = new UnityEngine.Vector3(0f, 1.5f, -5.5f + f * 0.04f);
        cam.transform.rotation = UnityEngine.Quaternion.LookRotation(
            UnityEngine.Quaternion.Euler(0f, f * 0.03f, 0f) * (new UnityEngine.Vector3(0f, -0.1f, 6f) - new UnityEngine.Vector3(0f, 1.5f, -5.5f + f * 0.04f)).normalized);
        var px = shoot();
        for (int i = 0; i < px.Length; i++)
        {
            if (f == 0) { lo[i * 3] = px[i].r; lo[i * 3 + 1] = px[i].g; lo[i * 3 + 2] = px[i].b; hi[i * 3] = px[i].r; hi[i * 3 + 1] = px[i].g; hi[i * 3 + 2] = px[i].b; }
            else
            {
                if (px[i].r < lo[i * 3]) lo[i * 3] = px[i].r; if (px[i].r > hi[i * 3]) hi[i * 3] = px[i].r;
                if (px[i].g < lo[i * 3 + 1]) lo[i * 3 + 1] = px[i].g; if (px[i].g > hi[i * 3 + 1]) hi[i * 3 + 1] = px[i].g;
                if (px[i].b < lo[i * 3 + 2]) lo[i * 3 + 2] = px[i].b; if (px[i].b > hi[i * 3 + 2]) hi[i * 3 + 2] = px[i].b;
            }
        }
    }
    int over24 = 0, over64 = 0, maxr = 0;
    var map = new UnityEngine.Color32[W * H];
    int[] cells = new int[16 * 9];
    for (int i = 0; i < W * H; i++)
    {
        int d = UnityEngine.Mathf.Max(hi[i * 3] - lo[i * 3], UnityEngine.Mathf.Max(hi[i * 3 + 1] - lo[i * 3 + 1], hi[i * 3 + 2] - lo[i * 3 + 2]));
        if (d > maxr) maxr = d;
        if (d > 24) over24++;
        if (d > 64) { over64++; cells[(8 - (i / W) / 75) * 16 + (i % W) / 75]++; }
        int amp = UnityEngine.Mathf.Min(255, d * 4);
        map[i] = new UnityEngine.Color32((byte)amp, (byte)amp, (byte)amp, 255);
    }
    if (saveMap)
    {
        var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
        t.SetPixels32(map); t.Apply();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shimmer-" + label + ".png"), t.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(t);
    }
    sb.AppendLine("  " + label.PadRight(22) + " |d|>24 = " + over24.ToString().PadLeft(7) + "   |d|>64 = " + over64.ToString().PadLeft(6) + "   max = " + maxr);
    if (saveMap)
        for (int row = 0; row < 9; row++)
        {
            var line = new System.Text.StringBuilder("      ");
            for (int col = 0; col < 16; col++)
            { int c = cells[row * 16 + col]; line.Append(c == 0 ? '.' : (c < 10 ? ':' : (c < 40 ? 'o' : (c < 120 ? 'O' : '#')))); }
            sb.AppendLine(line.ToString());
        }
    return new int[] { over24, over64, maxr };
};
try
{
    run("base", true);
    foreach (var m in water) m.SetFloat("_Ripple", 0f);
    run("noWaterRipple", false);
    for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", r0[k]);
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Smoothness", 0f);
    run("noGloss", false);
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Smoothness", s0[k]);
    foreach (var l in suns) l.shadows = UnityEngine.LightShadows.None;
    run("noSunShadows", false);
    for (int k = 0; k < suns.Count; k++) suns[k].shadows = sunShadow0[k];
    if (extra)
    {
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
        run("noAA", false);
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing;
        run("TAA", false);
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }
}
finally
{
    for (int k = 0; k < porcelain.Count; k++) { porcelain[k].SetFloat("_Caustics", c0[k]); porcelain[k].SetFloat("_Smoothness", s0[k]); }
    for (int k = 0; k < water.Count; k++) water[k].SetFloat("_Ripple", r0[k]);
    for (int k = 0; k < suns.Count; k++) suns[k].shadows = sunShadow0[k];
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
