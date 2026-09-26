// Render exactly once and report the flags URP actually sees for this camera.
var cam = UnityEngine.Camera.main;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
var rt = new UnityEngine.RenderTexture(400, 300, 24);
try { cam.targetTexture = rt; cam.Render(); }
finally { cam.targetTexture = null; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
return "rendered once"
    + " | cameraAA=" + extra.antialiasing
    + " | allowMsaa=" + extra.allowMsaa
    + " | allowHDR=" + extra.allowHDR
    + " | allowDynamicResolution=" + extra.allowDynamicResolution
    + " | assetMsaa=" + (urp ? urp.msaaSampleCount.ToString() : "-")
    + " | QualitySettings.antiAliasing=" + UnityEngine.QualitySettings.antiAliasing
    + " | renderPostProcessing=" + extra.renderPostProcessing;
