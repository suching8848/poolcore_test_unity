// Set the camera AA to None, render once, and report the per-camera flags.
// If "Disabling TAA" warnings stop appearing after this, the warning belongs to THIS camera
// (meaning TAA really is being disabled). If they keep appearing, it is another camera.
var cam = UnityEngine.Camera.main;
var extra = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
extra.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
var urp = UnityEngine.QualitySettings.renderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
var rt = new UnityEngine.RenderTexture(400, 300, 24);
try { cam.targetTexture = rt; cam.Render(); }
finally { cam.targetTexture = null; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
return "cameraAA=None applied"
    + " | allowMsaa=" + extra.allowMsaa
    + " | assetMsaa=" + (urp ? urp.msaaSampleCount.ToString() : "-")
    + " | qualityMsaa=" + UnityEngine.QualitySettings.antiAliasing
    + " | post=" + extra.renderPostProcessing;
