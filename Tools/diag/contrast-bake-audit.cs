var root=UnityEngine.GameObject.Find("Window Contrast Wing");int baked=0,missing=0;
foreach(var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>())if(r.gameObject.layer!=4){if(r.lightmapIndex>=0&&r.lightmapIndex<UnityEngine.LightmapSettings.lightmaps.Length)baked++;else missing++;}
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return new {baked,missing,worldLabels=UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>().Length,waterZones=UnityEngine.Object.FindObjectsByType<Poolcore.WaterZone>().Length};
