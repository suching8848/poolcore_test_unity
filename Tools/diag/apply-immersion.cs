if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play first");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.name!="Poolrooms")throw new System.Exception("Wrong scene");
System.IO.Directory.CreateDirectory("artifacts/immersion-audio");
System.IO.File.Copy(scene.path,"artifacts/immersion-audio/Before-label-removal.unity",true);
var removed=new System.Collections.Generic.List<string>();
foreach(var root in scene.GetRootGameObjects()) {
 foreach(var t in root.GetComponentsInChildren<TMPro.TextMeshPro>(true)) { removed.Add(t.name+": "+t.text);UnityEngine.Object.DestroyImmediate(t.gameObject); }
 foreach(var t in root.GetComponentsInChildren<UnityEngine.TextMesh>(true)) { removed.Add(t.name+": "+t.text);UnityEngine.Object.DestroyImmediate(t.gameObject); }
 foreach(var t in root.GetComponentsInChildren<UnityEngine.Transform>(true))if(t && t.name=="Hall Depth Marker")UnityEngine.Object.DestroyImmediate(t.gameObject);
}
var imported=new System.Collections.Generic.List<string>();
foreach(var file in System.IO.Directory.GetFiles("Assets/Poolcore/Resources/ImmersionAudio","*.wav")) {
 var path=file.Replace('\\','/');var a=(UnityEditor.AudioImporter)UnityEditor.AssetImporter.GetAtPath(path);
 a.forceToMono=true;var settings=a.defaultSampleSettings;settings.preloadAudioData=true;
 settings.loadType=UnityEngine.AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=UnityEngine.AudioCompressionFormat.PCM;
 settings.sampleRateSetting=UnityEditor.AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=24000;a.defaultSampleSettings=settings;a.SaveAndReimport();
 var clip=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AudioClip>(path);imported.Add(clip.name+" "+clip.length+"s "+clip.channels+"ch");
}
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);UnityEditor.AssetDatabase.SaveAssets();
return new {removed,imported};

