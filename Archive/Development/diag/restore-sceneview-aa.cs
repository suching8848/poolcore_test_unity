// Restore the Scene view's own anti-aliasing to its original value (it was 1) so this
// session leaves no unrequested editor-preference change behind.
var sb = new System.Text.StringBuilder();
var sv = UnityEditor.SceneView.lastActiveSceneView;
var t = typeof(UnityEditor.SceneView);
var bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var p1 = t.GetProperty("antiAliasing", bf);
if (sv != null && p1 != null && p1.CanWrite)
{
    sb.AppendLine("SceneView.antiAliasing " + p1.GetValue(sv) + " -> 1");
    p1.SetValue(sv, 1);
    sv.Repaint();
}
else sb.AppendLine("could not restore (property unavailable)");
// confirm the game camera still carries the fix
var cam = UnityEngine.Camera.main;
var extra = cam ? cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() : null;
sb.AppendLine("First Person Camera AA = " + (extra ? extra.antialiasing + " / " + extra.antialiasingQuality : "?"));
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
sb.AppendLine("runtime URP asset = " + (urp ? urp.name + " msaa=" + urp.msaaSampleCount : "null"));
return sb.ToString();
