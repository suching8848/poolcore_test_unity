// Clean Play Mode verification. Animation is frozen (water hidden, caustics off) so that a
// ticking editor cannot contaminate the diff, then TAA / SMAA / None are compared.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
if (!extra) return "ERROR: no camera data";
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
sb.AppendLine("isPlaying=" + UnityEngine.Application.isPlaying
    + "  clone=" + (urp ? urp.name + " msaa=" + urp.msaaSampleCount : "-")
    + "  QualitySettings.antiAliasing=" + UnityEngine.QualitySettings.antiAliasing);
sb.AppendLine("camera AA=" + extra.antialiasing + "/" + extra.antialiasingQuality);

var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
var porcelain = new System.Collections.Generic.List<UnityEngine.Material>();
var seen = new System.Collections.Generic.HashSet<UnityEngine.Material>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (m && m.shader.name == "Poolcore/Still Water") waters.Add(r);
    if (m && !seen.Contains(m) && m.shader.name == "Poolcore/Porcelain") { seen.Add(m); porcelain.Add(m); }
}
var c0 = new System.Collections.Generic.List<float>();
foreach (var m in porcelain) c0.Add(m.GetFloat("_Caustics"));

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
    int warm = extra.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing ? 8 : 1;
    var a = shootAt(warm, pose, q);
    var t = shootAt(warm, new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f), q);
    var r = shootAt(warm, pose, q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f));
    return "trans " + cmp(a, t) + "   rot " + cmp(a, r);
};
var aa0 = extra.antialiasing;
try
{
    foreach (var w in waters) w.enabled = false;
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", 0f);
    sb.AppendLine("(animation frozen: water hidden, caustics off)");
    sb.AppendLine("TAA (as fixed)   " + run());
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    sb.AppendLine("SMAA (was shipped) " + run());
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
    sb.AppendLine("None             " + run());
    extra.antialiasing = aa0;
}
finally
{
    extra.antialiasing = aa0;
    for (int k = 0; k < porcelain.Count; k++) porcelain[k].SetFloat("_Caustics", c0[k]);
    foreach (var w in waters) w.enabled = true;
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
