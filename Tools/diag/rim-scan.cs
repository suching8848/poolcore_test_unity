// Decisive rim scan: walk a world-space line across the atrium pool's west rim and
// read the rendered pixel at each step. Geometry (from PoolroomsBuilder.Pool):
//   deck "Atrium West"  covers x <= -7.00
//   rim  "Atrium West"  covers -7.08 .. -6.92   (0.16 m thick, centred on the pool edge)
//   => x in [-7.08,-7.00] is the COPLANAR OVERLAP of two different materials at y=0
//   => x in (-7.00,-6.92] is rim-only, x < -7.08 is deck-only
// If the overlap band mixes both materials and that mix changes with a 2 mm camera
// nudge, the rim is z-fighting.
var sb = new System.Text.StringBuilder();
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
var cam = UnityEngine.Camera.main;
const int W = 1600, H = 900;
var rt = new UnityEngine.RenderTexture(W, H, 24);
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (m && m.renderQueue >= 3000) waters.Add(r);
}
System.Func<UnityEngine.Color32[]> shoot = delegate
{
    cam.targetTexture = rt; cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t); return px;
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    foreach (var w in waters) w.enabled = false;

    cam.transform.position = new UnityEngine.Vector3(-5.6f, 2.4f, 3f);
    cam.transform.LookAt(new UnityEngine.Vector3(-7.02f, 0f, 3f));
    var shotA = shoot();
    cam.transform.position = new UnityEngine.Vector3(-5.602f, 2.4f, 3f);
    var shotB = shoot();

    // magnified crop of the rim as actually rendered
    int cx = (int)cam.WorldToScreenPoint(new UnityEngine.Vector3(-7.02f, 0f, 3f)).x;
    int cy = (int)cam.WorldToScreenPoint(new UnityEngine.Vector3(-7.02f, 0f, 3f)).y;
    int cw = 320, ch = 120;
    int x0 = UnityEngine.Mathf.Clamp(cx - cw / 2, 0, W - cw), y0 = UnityEngine.Mathf.Clamp(cy - ch / 2, 0, H - ch);
    var crop = new UnityEngine.Texture2D(cw * 3, ch * 3, UnityEngine.TextureFormat.RGB24, false);
    for (int y = 0; y < ch * 3; y++)
        for (int x = 0; x < cw * 3; x++)
            crop.SetPixel(x, y, shotA[(y0 + y / 3) * W + x0 + x / 3]);
    crop.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "rim-crop.png"), crop.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(crop);

    sb.AppendLine("camera (-5.6,2.4,3) -> look at rim (-7.02,0,3); rim band spans " + cw + "x" + ch + " px region at (" + x0 + "," + y0 + ")");
    sb.AppendLine("world x    screen(x,y)   zone          A(rgb)            B(rgb) 2mm-nudged   flip?");
    for (float wx = -6.94f; wx >= -7.20f; wx -= 0.01f)
    {
        var p = cam.WorldToScreenPoint(new UnityEngine.Vector3(wx, 0.002f, 3f));
        int px = UnityEngine.Mathf.RoundToInt(p.x), py = UnityEngine.Mathf.RoundToInt(p.y);
        string zone = wx <= -7.08f ? "deck-only" : (wx <= -7.0f ? "OVERLAP" : "rim-only");
        if (px < 0 || px >= W || py < 0 || py >= H) { sb.AppendLine(wx.ToString("F2") + "   off-screen"); continue; }
        var ca = shotA[py * W + px]; var cb = shotB[py * W + px];
        int dm = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(ca.r - cb.r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(ca.g - cb.g), UnityEngine.Mathf.Abs(ca.b - cb.b)));
        sb.AppendLine(string.Format("{0,7:F2}   ({1,4},{2,4})   {3,-12}  ({4,3},{5,3},{6,3})   ({7,3},{8,3},{9,3})   {10}",
            wx, px, py, zone, ca.r, ca.g, ca.b, cb.r, cb.g, cb.b, dm > 8 ? "YES d=" + dm : "-"));
    }
    foreach (var w in waters) w.enabled = true;
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st;
    UnityEngine.RenderTexture.active = sa; rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
