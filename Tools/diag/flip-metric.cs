// Locates the pool-edge flip: render at P and P+0.3 mm, count and map the pixels whose
// colour jumps. Also repeats with the water held out, to show how much of the flip
// lives on the water surface.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1000, H = 800;
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
    if (r.sharedMaterial && r.sharedMaterial.shader.name == "Poolcore/Still Water") waters.Add(r);

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
System.Func<string, UnityEngine.Color32[], UnityEngine.Color32[], UnityEngine.Color32[]> flip = delegate (string name, UnityEngine.Color32[] a, UnityEngine.Color32[] b)
{
    var map = new UnityEngine.Color32[W * H];
    int o32 = 0, o64 = 0, maxr = 0;
    int[] cells = new int[16 * 10];
    for (int i = 0; i < a.Length; i++)
    {
        int d = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - b[i].r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - b[i].g), UnityEngine.Mathf.Abs(a[i].b - b[i].b)));
        if (d > maxr) maxr = d;
        if (d > 32) o32++;
        if (d > 64) { o64++; cells[(9 - (i / W) / 80) * 16 + (i % W) / 63]++; }
        int amp = UnityEngine.Mathf.Min(255, d * 5);
        map[i] = new UnityEngine.Color32((byte)amp, (byte)amp, (byte)amp, 255);
    }
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.SetPixels32(map); t.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "flip-" + name + ".png"), t.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(t);
    sb.AppendLine("  " + name.PadRight(16) + " |d|>32 = " + o32.ToString().PadLeft(6) + "   |d|>64 = " + o64.ToString().PadLeft(5) + "   max = " + maxr);
    for (int row = 0; row < 10; row++)
    {
        var line = new System.Text.StringBuilder("      ");
        for (int col = 0; col < 16; col++)
        { int c = cells[row * 16 + col]; line.Append(c == 0 ? '.' : (c < 5 ? ':' : (c < 20 ? 'o' : (c < 60 ? 'O' : '#')))); }
        sb.AppendLine(line.ToString());
    }
    return map;
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    // same framing the user screenshotted: water left, drain + deck right, looking down
    cam.transform.position = new UnityEngine.Vector3(9.0f, 1.8f, 0.0f);
    cam.transform.LookAt(new UnityEngine.Vector3(7.0f, -0.3f, 3.5f));
    var a = shoot();
    cam.transform.position = new UnityEngine.Vector3(9.0003f, 1.8f, 0.0003f);
    var b = shoot();
    flip("base", a, b);
    foreach (var w in waters) w.enabled = false;
    var a2 = shoot();
    cam.transform.position = new UnityEngine.Vector3(9.0f, 1.8f, 0.0f);
    var b2 = shoot();
    flip("noWater", a2, b2);
    foreach (var w in waters) w.enabled = true;
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
