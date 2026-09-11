// Aliasing probe, independent of _Time: render at 1x and at 2x, box-downsample the 2x
// image, then diff. A pattern that survives supersampling (geometry edges, flat shading)
// matches; a thin high-frequency procedural pattern (caustic filaments, grout lines)
// does not - and that mismatch is exactly what shimmers/flickers under camera motion.
var sb = new System.Text.StringBuilder();
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1600, H = 900;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

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
System.Action<UnityEngine.Color32[], string> save = delegate (UnityEngine.Color32[] px, string name)
{
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.SetPixels32(px); t.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name), t.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(t);
};
System.Action<string> probe = delegate (string label)
{
    var a = render(W, H);
    var big = render(W * 2, H * 2);
    var d = new UnityEngine.Color32[W * H];
    int over12 = 0, over40 = 0, maxd = 0;
    int[] cells = new int[16 * 9];
    for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            int i = y * W + x;
            int r = 0, g = 0, b = 0;
            for (int dy = 0; dy < 2; dy++)
                for (int dx = 0; dx < 2; dx++)
                {
                    var p = big[(y * 2 + dy) * (W * 2) + x * 2 + dx];
                    r += p.r; g += p.g; b += p.b;
                }
            r /= 4; g /= 4; b /= 4;
            int m = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - g), UnityEngine.Mathf.Abs(a[i].b - b)));
            if (m > maxd) maxd = m;
            if (m > 12) over12++;
            if (m > 40) { over40++; cells[(8 - y / 100) * 16 + x / 100]++; }
            int amp = UnityEngine.Mathf.Min(255, m * 8);
            d[i] = new UnityEngine.Color32((byte)amp, (byte)amp, (byte)amp, 255);
        }
    save(d, "alias-" + label + "-diff.png");
    sb.AppendLine("--- " + label + ": 1x vs 2x-downsampled   |d|>12 = " + over12 + "   |d|>40 = " + over40 + "   max = " + maxd);
    for (int row = 0; row < 9; row++)
    {
        var line = new System.Text.StringBuilder("    ");
        for (int col = 0; col < 16; col++)
        {
            int c = cells[row * 16 + col];
            line.Append(c == 0 ? '.' : (c < 5 ? ':' : (c < 20 ? 'o' : (c < 60 ? 'O' : '#'))));
        }
        sb.AppendLine(line.ToString());
    }
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }
    cam.transform.position = new UnityEngine.Vector3(0f, 1.71f, -10f);
    cam.transform.rotation = UnityEngine.Quaternion.identity;
    probe("atrium-wide");
    cam.transform.position = new UnityEngine.Vector3(0f, 1.5f, -5.5f);
    cam.transform.LookAt(new UnityEngine.Vector3(0f, -0.1f, 6f));
    probe("waterline");
    if (extra) { extra.renderPostProcessing = post; extra.antialiasing = aa; }
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
}
return sb.ToString();
