// Pool-edge z-fighting probe.
// (1) beauty capture at the canonical player viewpoint (post-processing ON)
// (2) z-fight energy: post-processing + water OFF, render at P and P+0.4mm, diff
//     Static opaque geometry + frozen _Time means any high-contrast difference is a
//     depth-test flip between coplanar surfaces.
var sb = new System.Text.StringBuilder();
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
System.IO.Directory.CreateDirectory(dir);

var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main in the open scene";
string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
sb.AppendLine("scene: " + scene + "   camera: " + cam.name);

bool raised = false;
var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None))
{
    var m = r.sharedMaterial;
    if (m && m.name == "Sea Glass Mosaic" && r.bounds.max.y > 0.02f) raised = true;
    if (m && m.renderQueue >= 3000) waters.Add(r);
}
string tag = raised ? "after" : "before";
sb.AppendLine("tag: " + tag + "   transparent renderers held out of probe: " + waters.Count);

var rt = new UnityEngine.RenderTexture(1600, 900, 24);
var savedPos = cam.transform.position;
var savedRot = cam.transform.rotation;
var savedTarget = cam.targetTexture;
var savedActive = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

System.Func<UnityEngine.RenderTexture, UnityEngine.Color32[]> shoot = delegate (UnityEngine.RenderTexture target)
{
    cam.targetTexture = target;
    cam.Render();
    var prev = UnityEngine.RenderTexture.active;
    UnityEngine.RenderTexture.active = target;
    var t = new UnityEngine.Texture2D(1600, 900, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, 1600, 900), 0, 0);
    t.Apply();
    var px = t.GetPixels32();
    UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t);
    return px;
};
System.Action<UnityEngine.Color32[], string> save = delegate (UnityEngine.Color32[] px, string name)
{
    var t = new UnityEngine.Texture2D(1600, 900, UnityEngine.TextureFormat.RGB24, false);
    t.SetPixels32(px); t.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name), t.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(t);
};

try
{
    // (1) beauty capture, canonical viewpoint, full post-processing
    cam.transform.position = new UnityEngine.Vector3(0f, 1.71f, -10f);
    cam.transform.rotation = UnityEngine.Quaternion.identity;
    save(shoot(rt), "edge-" + tag + ".png");

    // (2) z-fight probe
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    foreach (var w in waters) w.enabled = false;
    var a = shoot(rt);
    cam.transform.position = new UnityEngine.Vector3(0.0004f, 1.71f, -10.0004f);
    var b = shoot(rt);
    cam.transform.position = new UnityEngine.Vector3(0f, 1.71f, -10f);
    foreach (var w in waters) w.enabled = true;
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }

    var d = new UnityEngine.Color32[1600 * 900];
    int over12 = 0, over40 = 0, maxd = 0;
    int minX = 9999, maxX = -1, minY = 9999, maxY = -1;
    for (int i = 0; i < d.Length; i++)
    {
        int dr = UnityEngine.Mathf.Abs(a[i].r - b[i].r);
        int dg = UnityEngine.Mathf.Abs(a[i].g - b[i].g);
        int db = UnityEngine.Mathf.Abs(a[i].b - b[i].b);
        int m = UnityEngine.Mathf.Max(dr, UnityEngine.Mathf.Max(dg, db));
        if (m > maxd) maxd = m;
        if (m > 12) over12++;
        if (m > 40)
        {
            over40++;
            int x = i % 1600, y = i / 1600;
            if (x < minX) minX = x; if (x > maxX) maxX = x;
            if (y < minY) minY = y; if (y > maxY) maxY = y;
        }
        int amp = UnityEngine.Mathf.Min(255, m * 8);
        d[i] = new UnityEngine.Color32((byte)amp, (byte)amp, (byte)amp, 255);
    }
    save(d, "edge-" + tag + "-diff.png");
    sb.AppendLine("probe pixels |d|>12 : " + over12 + "  (" + (100f * over12 / d.Length).ToString("F3") + "% of frame)");
    sb.AppendLine("probe pixels |d|>40 : " + over40 + "   max delta: " + maxd);
    if (over40 > 0) sb.AppendLine("high-contrast diff bbox: x " + minX + ".." + maxX + "  y " + minY + ".." + maxY + "  (image y=0 is bottom)");
}
finally
{
    cam.transform.position = savedPos;
    cam.transform.rotation = savedRot;
    cam.targetTexture = savedTarget;
    UnityEngine.RenderTexture.active = savedActive;
    rt.Release();
    UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
