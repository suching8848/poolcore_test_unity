// Fix: the Poolrooms camera used SMAA, which is a purely spatial filter and cannot remove
// view-motion shimmer. TAA is the targeted remedy (measured 6x fewer flipping pixels).
var cam = UnityEngine.Camera.main;
if (cam == null) return "ERROR: no Camera.main";
var data = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
if (!data) return "ERROR: Camera.main has no UniversalAdditionalCameraData";
string before = data.antialiasing + " / " + data.antialiasingQuality;
UnityEditor.Undo.RecordObject(data, "Poolrooms: temporal antialiasing");
data.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.TemporalAntiAliasing;
data.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.High;
UnityEditor.EditorUtility.SetDirty(data);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "camera=" + cam.name
     + "\nAA before = " + before
     + "\nAA after  = " + data.antialiasing + " / " + data.antialiasingQuality
     + "\nscene saved = " + saved + "   (" + scene.path + ")"
     + "\nprefab instance overrides present = " + (UnityEditor.PrefabUtility.GetPrefabInstanceStatus(cam.gameObject) != UnityEditor.PrefabInstanceStatus.NotAPrefab);
