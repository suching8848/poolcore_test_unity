// Specular-aliasing test: the pool rim and drain are glossy porcelain at a grazing angle
// to the sun, so their specular highlight can fall below one pixel and jump between
// neighbouring pixels when the view moves. Sweep _Smoothness with the shipping post stack
// (post ON, SMAA) and watch the flip count collapse if that is the cause.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1000, H = 800;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

var coping = new System.Collections.Generic.List<UnityEngine.Material>();
var drain = new System.Collections.Generic.List<UnityEngine.Material>();
var seen = new System.Collections.Generic.HashSet<UnityEngine.Material>();
foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>())
{
    var m = r.sharedMaterial;
    if (!m || seen.Contains(m)) continue;
    seen.Add(m);
    if (m.name == "Sea Glass Mosaic") coping.Add(m);
    if (m.name == "Deep Green Trim") drain.Add(m);
}
var all = new System.Collections.Generic.List<UnityEngine.Material>();
foreach (var m in coping) all.Add(m);
foreach (var m in drain) if (!all.Contains(m)) all.Add(m);
var s0 = new System.Collections.Generic.List<float>();
foreach (var m in all) s0.Add(m.GetFloat("_Smoothness"));
var names = new System.Text.StringBuilder();
foreach (var m in all) names.Append(m.name + "=" + m.GetFloat("_Smoothness") + " ");
sb.AppendLine("glossy pool materials: " + names.ToString());

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
System.Func<string> measure = delegate
{
    var q = UnityEngine.Quaternion.LookRotation(look - pose);
    cam.transform.position = pose; cam.transform.rotation = q;
    var a = shoot();
    cam.transform.position = new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f);
    var t = shoot();
    cam.transform.position = pose; cam.transform.rotation = q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f);
    var r = shoot();
    cam.transform.position = pose; cam.transform.rotation = q;
    return "trans " + cmp(a, t) + "   rot " + cmp(a, r);
};
try
{
    // shipping stack: post-processing ON, SMAA as authored
    sb.AppendLine("base (post ON, SMAA)  " + measure());
    for (int k = 0; k < all.Count; k++) all[k].SetFloat("_Smoothness", 0.45f);
    sb.AppendLine("_Smoothness = 0.45     " + measure());
    for (int k = 0; k < all.Count; k++) all[k].SetFloat("_Smoothness", 0.25f);
    sb.AppendLine("_Smoothness = 0.25     " + measure());
    for (int k = 0; k < all.Count; k++) all[k].SetFloat("_Smoothness", 0.0f);
    sb.AppendLine("_Smoothness = 0.00     " + measure());
    for (int k = 0; k < all.Count; k++) all[k].SetFloat("_Smoothness", s0[k]);
    sb.AppendLine("restored               " + measure());
}
finally
{
    for (int k = 0; k < all.Count; k++) all[k].SetFloat("_Smoothness", s0[k]);
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
