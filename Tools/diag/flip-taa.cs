// Does TAA (with temporal history) actually remove the view-motion flip?
// TAA needs frames to accumulate, so each pose is rendered `warm` times before capture.
// If the flip count collapses with TAA, temporal antialiasing is the remedy.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
const int W = 1000, H = 800;
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
if (!extra) return "ERROR: no UniversalAdditionalCameraData";

var rt = new UnityEngine.RenderTexture(W, H, 24);
var pose = new UnityEngine.Vector3(9.0f, 1.8f, 0.0f);
var look = new UnityEngine.Vector3(7.0f, -0.3f, 3.5f);
var q = UnityEngine.Quaternion.LookRotation(look - pose);

System.Func<int, UnityEngine.Vector3, UnityEngine.Quaternion, UnityEngine.Color32[]> shootAt = delegate (int warm, UnityEngine.Vector3 p, UnityEngine.Quaternion rot)
{
    cam.transform.position = p; cam.transform.rotation = rot;
    cam.targetTexture = rt;
    for (int i = 0; i < warm; i++) cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    var px = t.GetPixels32(); UnityEngine.RenderTexture.active = prev;
    UnityEngine.Object.DestroyImmediate(t);
    cam.targetTexture = null;
    return px;
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
System.Func<int, string> run = delegate (int warm)
{
    var a = shootAt(warm, pose, q);
    var t = shootAt(warm, new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f), q);
    var r = shootAt(warm, pose, q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f));
    return "trans " + cmp(a, t) + "   rot " + cmp(a, r);
};
try
{
    extra.renderPostProcessing = true;
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    sb.AppendLine("SMAA (shipping)  warm=1   " + run(1));
    sb.AppendLine("SMAA             warm=8   " + run(8));
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing;
    sb.AppendLine("TAA              warm=1   " + run(1));
    sb.AppendLine("TAA              warm=4   " + run(4));
    sb.AppendLine("TAA              warm=8   " + run(8));
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
    sb.AppendLine("None             warm=8   " + run(8));
    extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
