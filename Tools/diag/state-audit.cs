// State audit: camera transform (probes must have restored it), camera AA, URP asset MSAA,
// and the last build status — so we do not tell the user to save a displaced camera.
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
var extra = cam ? cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() : null;
sb.AppendLine("Camera.main          = " + (cam ? cam.name : "NULL"));
if (cam)
{
    sb.AppendLine("  world position     = " + cam.transform.position.ToString("F4") + "   (authored spawn view is (0, 1.71, -10))");
    sb.AppendLine("  world rotation     = " + cam.transform.rotation.eulerAngles.ToString("F3"));
    sb.AppendLine("  local position     = " + cam.transform.localPosition.ToString("F4"));
    var player = UnityEngine.Object.FindAnyObjectByType<Poolcore.FirstPersonController>();
    if (player) sb.AppendLine("  player position    = " + player.transform.position.ToString("F4") + "   rotation " + player.transform.rotation.eulerAngles.ToString("F3"));
}
if (extra) sb.AppendLine("camera AA            = " + extra.antialiasing + " / " + extra.antialiasingQuality);
var assigned = UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset;
sb.AppendLine("URP asset (active)   = " + (assigned ? assigned.name + "  msaa=" + assigned.msaaSampleCount + "  renderScale=" + assigned.renderScale : "NULL"));
sb.AppendLine("QualitySettings.renderPipeline = " + (UnityEngine.QualitySettings.renderPipeline ? UnityEngine.QualitySettings.renderPipeline.name : "null"));
sb.AppendLine("QualitySettings.antiAliasing  = " + UnityEngine.QualitySettings.antiAliasing);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
sb.AppendLine("active scene         = " + scene.name + "  dirty=" + scene.isDirty);
sb.AppendLine("isPlaying            = " + UnityEngine.Application.isPlaying);
// camera position on disk, for comparison against the in-memory value above
var path = "Assets/Poolcore/Scenes/Poolrooms.unity";
sb.AppendLine("scene asset path     = " + scene.path + "   (on-disk file " + path + ")");
return sb.ToString();
