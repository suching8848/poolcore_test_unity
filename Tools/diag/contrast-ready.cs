var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
if(UnityEditor.SceneView.lastActiveSceneView && UnityEngine.Camera.main)UnityEditor.SceneView.lastActiveSceneView.AlignViewToObject(UnityEngine.Camera.main.transform);
var logType=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.LogEntries");var clear=logType?.GetMethod("Clear",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);clear?.Invoke(null,null);
return new {scene=scene.name,playing=UnityEditor.EditorApplication.isPlaying,dirty=scene.isDirty,spawn=UnityEngine.GameObject.Find("Player").transform.position};
