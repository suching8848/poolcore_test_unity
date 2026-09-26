// Pool-edge flicker probe at REALISTIC per-frame motion (60 fps: ~4 cm walk, ~0.25 deg look).
// Post-processing and water are held out, _Time is frozen inside one tick, so any
// high-contrast difference between the two renders is a depth-test flip.
var sb = new System.Text.StringBuilder();
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
System.IO.Directory.CreateDirectory(dir);
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
bool raised = false;
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (m && m.name == "Sea Glass Mosaic" && r.bounds.max.y > 0.02f) raised = true;
    if (m && m.renderQueue >= 3000) waters.Add(r);
}
string tag = raised ? "after" : "before";
sb.AppendLine("scene " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "  tag " + tag);

const int W = 1600, H = 900;
var rt = new UnityEngine.RenderTexture(W, H, 24);
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var home = new UnityEngine.Vector3(0f, 1.71f, -10f);

System.Func<UnityEngine.RenderTexture, UnityEngine.Color32[]> shoot = delegate (UnityEngine.RenderTexture target)
{
    cam.targetTexture = target; cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = target;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t); return px;
};
System.Action<UnityEngine.Color32[], string> save = delegate (UnityEngine.Color32[] px, string name)
{
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.SetPixels32(px); t.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name), t.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(t);
};
System.Action<string, UnityEngine.Color32[], UnityEngine.Color32[]> report = delegate (string label, UnityEngine.Color32[] a, UnityEngine.Color32[] b)
{
    var d = new UnityEngine.Color32[W * H];
    int over12 = 0, over40 = 0, maxd = 0;
    int[] cells = new int[16 * 9];
    for (int i = 0; i < d.Length; i++)
    {
        int m = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - b[i].r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - b[i].g), UnityEngine.Mathf.Abs(a[i].b - b[i].b)));
        if (m > maxd) maxd = m;
        if (m > 12) over12++;
        if (m > 40)
        {
            over40++;
            int x = i % W, y = i / W;
            cells[(8 - y / 100) * 16 + x / 100]++;
        }
        int amp = UnityEngine.Mathf.Min(255, m * 8);
        d[i] = new UnityEngine.Color32((byte)amp, (byte)amp, (byte)amp, 255);
    }
    save(d, "edge-" + tag + "-diff-" + label + ".png");
    sb.AppendLine("--- " + label + ": |d|>12 = " + over12 + "   |d|>40 = " + over40 + "   max = " + maxd);
    for (int row = 0; row < 9; row++)
    {
        var line = new System.Text.StringBuilder("    ");
        for (int col = 0; col < 16; col++)
        {
            int c = cells[row * 16 + col];
            line.Append(c == 0 ? '.' : (c < 3 ? ':' : (c < 10 ? 'o' : (c < 40 ? 'O' : '#'))));
        }
        sb.AppendLine(line.ToString());
    }
};
try
{
    cam.transform.position = home; cam.transform.rotation = UnityEngine.Quaternion.identity;
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    foreach (var w in waters) w.enabled = false;
    var baseFrame = shoot(rt);
    cam.transform.position = new UnityEngine.Vector3(0.04f, 1.71f, -10.04f);
    report("walk4cm", baseFrame, shoot(rt));
    cam.transform.position = home;
    cam.transform.rotation = UnityEngine.Quaternion.Euler(0f, 0.25f, 0f);
    report("turn025deg", baseFrame, shoot(rt));
    cam.transform.position = home; cam.transform.rotation = UnityEngine.Quaternion.identity;
    foreach (var w in waters) w.enabled = true;
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st;
    UnityEngine.RenderTexture.active = sa; rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
