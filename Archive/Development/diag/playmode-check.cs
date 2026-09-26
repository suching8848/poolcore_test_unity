// Play Mode reality check: is TAA actually in effect once ExperienceMenu has run?
// Reports the camera AA, whether the runtime URP clone re-enabled MSAA (URP TAA and MSAA
// are mutually exclusive), and re-measures the pool-rim view-motion flip.
var sb = new System.Text.StringBuilder();
sb.AppendLine("isPlaying        = " + UnityEngine.Application.isPlaying);
sb.AppendLine("QualitySettings.antiAliasing (MSAA) = " + UnityEngine.QualitySettings.antiAliasing);
var pipe = UnityEngine.QualitySettings.renderPipeline;
sb.AppendLine("renderPipeline   = " + (pipe ? pipe.GetType().Name + " '" + pipe.name + "'" : "null (built-in default)"));
var urp = pipe as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
if (urp) sb.AppendLine("URP msaaSampleCount / renderScale / shadowDistance = " + urp.msaaSampleCount + " / " + urp.renderScale + " / " + urp.shadowDistance);
sb.AppendLine("UniversalRenderPipeline.asset == QualitySettings.renderPipeline : "
    + (UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset == urp));

var cam = UnityEngine.Camera.main;
sb.AppendLine("Camera.main      = " + (cam ? cam.name : "NULL"));
var extra = cam ? cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() : null;
if (!extra) return sb.ToString() + "\nERROR: no UniversalAdditionalCameraData on Camera.main";
sb.AppendLine("camera AA        = " + extra.antialiasing + " / " + extra.antialiasingQuality + "   post=" + extra.renderPostProcessing);

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
try
{
    int warm = extra.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing ? 8 : 1;
    var a = shootAt(warm, pose, q);
    var t = shootAt(warm, new UnityEngine.Vector3(pose.x + 0.0003f, pose.y, pose.z + 0.0003f), q);
    var r = shootAt(warm, pose, q * UnityEngine.Quaternion.Euler(0f, 0.02f, 0f));
    sb.AppendLine("trans(0.3mm)  " + cmp(a, t));
    sb.AppendLine("rot(0.02deg)  " + cmp(a, r));
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return sb.ToString();
