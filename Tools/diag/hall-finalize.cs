var player=UnityEngine.Object.FindAnyObjectByType<Poolcore.FirstPersonController>();player.transform.position=new UnityEngine.Vector3(58,.06f,0);player.transform.rotation=UnityEngine.Quaternion.identity;player.view.transform.localRotation=UnityEngine.Quaternion.identity;
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
return UnityEditor.EditorApplication.ExecuteMenuItem("Poolcore/Bake Soft Lighting");
