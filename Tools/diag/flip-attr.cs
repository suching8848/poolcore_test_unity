// Attribution of the pool-edge flip. For each material/lighting configuration, measure how
// many pixels jump when the camera makes a REALISTIC per-frame move (0.3 mm translate or
// 0.02 deg yaw). Configs that drop the count identify the source.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1000, H = 800;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

var waters = new System.Collections.Generic.List<UnityEngine.Renderer>();
var coping = new System.Collections.Generic.List<UnityEngine.Renderer>();
var drains = new System.Collections.Generic.List<UnityEngine.Renderer>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (m && m.shader.name == "Poolcore/Still Water") waters.Add(r);
    if (m && m.name == "Sea Glass Mosaic") coping.Add(r);
    if (r.name.Contains("Drain")) drains.Add(r);
}
var suns = new System.Collections.Generic.List<UnityEngine.Light>();
foreach (var l in UnityEngine.Object.FindObjectsByType<UnityEngine.Light>())
    if (l.type == UnityEngine.LightType.Directional) suns.Add(l);
var sh0 = new System.Collections.Generic.List<UnityEngine.LightShadows>();
foreach (var l in suns) sh0.Add(l.shadows);
sb.AppendLine("water=" + waters.Count + " coping=" + coping.Count + " drains=" + drains.Count + " suns=" + suns.Count);

var rt = new UnityEngine.RenderTexture(W, H, 24);
var pose = new UnityEngine.Vector3(9.0f, 1.8f, 0.0f);
var look = new UnityEngine.Vector3(7.0f, -0.3f, 3.5f);
System.Func<UnityEngine.Color32[]> shoot = delegate
{
    cam.targetTexture = rt; cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t); return px;
};
System.Func<UnityEngine.Color32[], UnityEngine.Color32[], int[]> cmp = delegate (UnityEngine.Color32[] a, UnityEngine.Color32[] b)
{
    int o16 = 0, o32 = 0, o64 = 0, maxr = 0;
    for (int i = 0; i < a.Length; i++)
    {
        int d = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].r - b[i].r), UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(a[i].g - b[i].g), UnityEngine.Mathf.Abs(a[i].b - b[i].b)));
        if (d > maxr) maxr = d;
        if (d > 16) o16++;
        if (d > 32) o32++;
        if (d > 64) o64++;
    }
    return new int[] { o16, o32, o64, maxr };
};
System.Func<string> measure = delegate
{
    cam.transform.position = pose; cam.transform.LookAt(look);
    var a = shoot();
    cam.transform.position = new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f);
    var bT = shoot();
    cam.transform.position = pose; cam.transform.rotation = UnityEngine.Quaternion.LookRotation(look - pose);
    var q = cam.transform.rotation;
    cam.transform.rotation = q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f);
    var bR = shoot();
    cam.transform.rotation = q;
    var t = cmp(a, bT); var r = cmp(a, bR);
    return "  trans(0.3mm) >16/32/64 = " + t[0] + "/" + t[1] + "/" + t[2] + " max " + t[3]
         + "   rot(0.02deg) >16/32/64 = " + r[0] + "/" + r[1] + "/" + r[2] + " max " + r[3];
};
try
{
    bool post = extra ? extra.renderPostProcessing : false;
    var aa = extra ? extra.antialiasing : UnityEngine.Rendering.Universal.AntialiasingMode.None;
    if (extra) { extra.renderPostProcessing = false; extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None; }

    sb.AppendLine("base          " + measure());
    foreach (var w in waters) w.enabled = false; sb.AppendLine("noWater       " + measure()); foreach (var w in waters) w.enabled = true;
    foreach (var w in coping) w.enabled = false; sb.AppendLine("noCoping      " + measure()); foreach (var w in coping) w.enabled = true;
    foreach (var w in drains) w.enabled = false; sb.AppendLine("noDrain       " + measure()); foreach (var w in drains) w.enabled = true;
    foreach (var l in suns) l.shadows = UnityEngine.LightShadows.None; sb.AppendLine("noSunShadows  " + measure()); for (int k = 0; k < suns.Count; k++) suns[k].shadows = sh0[k];

    if (extra)
    {
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing;
        sb.AppendLine("TAA           " + measure());
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
        sb.AppendLine("noAA          " + measure());
        extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        extra.renderPostProcessing = true;
        sb.AppendLine("postProcessing" + measure());
        extra.renderPostProcessing = post;
    }
    if (extra) extra.antialiasing = aa;
}
finally
{
    for (int k = 0; k < suns.Count; k++) suns[k].shadows = sh0[k];
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
