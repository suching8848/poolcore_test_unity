foreach(string name in new[]{"Entry Slope","Shallow Shelf","Depth Transition","Deep Basin"}) {
var mesh=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>("Assets/Poolcore/Meshes/Hall "+name+".asset");var so=new UnityEditor.SerializedObject(mesh);so.FindProperty("m_PreBakeTriangleCollisionMesh").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();UnityEditor.EditorUtility.SetDirty(mesh);
}
UnityEditor.AssetDatabase.SaveAssets();return "Enabled triangle collision prebake on all four basin meshes";
