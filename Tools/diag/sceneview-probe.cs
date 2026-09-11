// Which editor camera emits "Disabling TAA because MSAA is on"?
var sb = new System.Text.StringBuilder();
var sv = UnityEditor.SceneView.lastActiveSceneView;
sb.AppendLine("lastActiveSceneView = " + (sv ? sv.name : "null"));
if (sv && sv.camera)
{
    var cd = sv.camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    sb.AppendLine("  sceneView camera = " + sv.camera.name + "   UACD=" + (cd ? cd.antialiasing.ToString() : "none"));
}
sb.AppendLine("QualitySettings.antiAliasing = " + UnityEngine.QualitySettings.antiAliasing);
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
sb.AppendLine("active URP asset = " + (urp ? urp.name + " msaa=" + urp.msaaSampleCount : "null"));
var cs = sv ? sv.cameraSettings : null;
if (cs != null)
{
    var t = cs.GetType();
    sb.AppendLine("SceneView.CameraSettings members:");
    foreach (var p in t.GetProperties())
    {
        string v;
        try { var o = p.GetValue(cs); v = o == null ? "null" : o.ToString(); } catch { v = "<err>"; }
        sb.AppendLine("    " + p.Name + " = " + v);
    }
}
var cameras = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(UnityEngine.FindObjectsInactive.Include);
sb.AppendLine("cameras in loaded scenes = " + cameras.Length);
foreach (var c in cameras)
{
    var cd = c.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    sb.AppendLine("    " + c.name + "  type=" + c.cameraType + "  enabled=" + c.enabled + "  AA=" + (cd ? cd.antialiasing.ToString() : "none"));
}
return sb.ToString();
