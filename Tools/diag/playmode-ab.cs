// Play Mode A/B of the antialiasing mode at the shipping MSAA level, plus state reporting
// so a moving parent (player falling) or an advancing clock cannot be mistaken for shimmer.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
var extra = cam ? cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() : null;
if (!extra) return "ERROR: no camera data";
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
var player = UnityEngine.Object.FindAnyObjectByType<Poolcore.FirstPersonController>();
sb.AppendLine("MSAA=" + UnityEngine.QualitySettings.antiAliasing + "  urp.msaa=" + (urp ? urp.msaaSampleCount : -1)
    + "  urp.name=" + (urp ? urp.name : "-"));
sb.AppendLine("cursor=" + UnityEngine.Cursor.lockState + "  player=" + (player ? player.transform.position.ToString("F3") : "none"));
sb.AppendLine("frame=" + UnityEngine.Time.frameCount + "  time=" + UnityEngine.Time.time.ToString("F3"));

const int W = 1000, H = 800;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var rt = new UnityEngine.RenderTexture(W, H, 24);
var pose = new UnityEngine.Vector3(9.0f, 1.8f, 0.0f);
var look = new UnityEngine.Vector3(7.0f, -0.3f, 3.5f);
var q = UnityEngine.Quaternion.LookRotation(look - pose);
System.Func<int, UnityEngine.Vector3, UnityEngine.Quaternion, UnityEngine.Color32[]> shootAt = delegate (int warm, UnityEngine.Vector3 p, UnityEngine.Quaternion rot)
{
    cam.transform.position = p; cam.transform.rotation = rot; cam.targetTexture = rt;
    for (int i = 0; i < warm; i++) cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t); cam.targetTexture = null; return px;
};
System.Func<UnityEngine.Color32[], UnityEngine.Color32[], string> cmp = delegate (UnityEngine.Color32[] a, UnityEngine.Color32[] b)
{
    int o16 = 0, o32 = 0, o64 = 0, maxr = 0;
    for (int i = 0; i < a.Length; i++)
    {
        int d = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - b[i].r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - b[i].g), UnityEngine.Mathf.Abs(a[i].b - b[i].b)));
        if (d > maxr) maxr = d;
        if (d > 16) o16++; if (d > 32) o32++; if (d > 64) o64++;
    }
    return ">16/32/64 = " + o16 + "/" + o32 + "/" + o64 + "  max " + maxr;
};
System.Func<string> run = delegate
{
    var a = shootAt(8, pose, q);
    var t = shootAt(8, new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f), q);
    var r = shootAt(8, pose, q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f));
    var pa = player ? player.transform.position : UnityEngine.Vector3.zero;
    return "trans " + cmp(a, t) + "   rot " + cmp(a, r) + "   playerY=" + pa.y.ToString("F4");
};
var aa0 = extra.antialiasing;
try
{
    sb.AppendLine("TAA   " + run());
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    sb.AppendLine("SMAA  " + run());
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
    sb.AppendLine("None  " + run());
    extra.antialiasing = aa0;
    // MSAA off, TAA on
    if (urp) { urp.msaaSampleCount = 1; sb.AppendLine("TAA + msaa=1  " + run()); }
}
finally
{
    extra.antialiasing = aa0;
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
