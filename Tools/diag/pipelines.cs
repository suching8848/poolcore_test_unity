// Confirm every render-pipeline asset the player could use, and their MSAA level, so the
// build cannot silently re-enable MSAA and disable TAA again.
var sb = new System.Text.StringBuilder();
var g = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
sb.AppendLine("GraphicsSettings.defaultRenderPipeline = " + (g ? g.name + " (" + g.GetType().Name + ")" : "null"));
var gq = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
sb.AppendLine("GraphicsSettings.currentRenderPipeline = " + (gq ? gq.name : "null"));
sb.AppendLine("QualitySettings.renderPipeline       = " + (UnityEngine.QualitySettings.renderPipeline ? UnityEngine.QualitySettings.renderPipeline.name : "null"));
sb.AppendLine("current quality level                = " + UnityEngine.QualitySettings.GetQualityLevel() + " / " + UnityEngine.QualitySettings.names.Length);
for (int i = 0; i < UnityEngine.QualitySettings.names.Length; i++)
{
    int prev = UnityEngine.QualitySettings.GetQualityLevel();
    UnityEngine.QualitySettings.SetQualityLevel(i, false);
    var rp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
    sb.AppendLine("  level " + i + " '" + UnityEngine.QualitySettings.names[i] + "'  msaa=" + UnityEngine.QualitySettings.antiAliasing
        + "  renderPipeline=" + (rp ? rp.name + " msaa=" + rp.msaaSampleCount : "null"));
    UnityEngine.QualitySettings.SetQualityLevel(prev, false);
}
sb.AppendLine("--- all URP assets in project ---");
foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
{
    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    var a = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(path);
    if (a) sb.AppendLine("  " + path + "   msaa=" + a.msaaSampleCount + "  renderScale=" + a.renderScale + "  shadowDistance=" + a.shadowDistance + "  HDR=" + a.supportsHDR);
}
return sb.ToString();
