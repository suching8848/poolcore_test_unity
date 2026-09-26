// The Scene view has its own anti-aliasing setting, independent of the game camera and the
// URP asset. Locate it so we can tell the user exactly how to stop URP from disabling TAA
// in the Scene view.
var sb = new System.Text.StringBuilder();
var sv = UnityEditor.SceneView.lastActiveSceneView;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
          | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
var t = typeof(UnityEditor.SceneView);
sb.AppendLine("--- SceneView fields mentioning aa/msaa/anti ---");
foreach (var f in t.GetFields(flags))
{
    var n = f.Name.ToLowerInvariant();
    if (n.Contains("msaa") || n.Contains("antialias") || n == "maa" || n.Contains("multisampl"))
    {
        string v;
        try { v = f.IsStatic ? (f.GetValue(null) == null ? "null" : f.GetValue(null).ToString()) : (sv != null && f.GetValue(sv) != null ? f.GetValue(sv).ToString() : "null"); }
        catch { v = "<err>"; }
        sb.AppendLine("  " + f.Name + " : " + f.FieldType.Name + " = " + v);
    }
}
sb.AppendLine("--- SceneView properties mentioning aa/msaa/anti ---");
foreach (var p in t.GetProperties(flags))
{
    var n = p.Name.ToLowerInvariant();
    if (n.Contains("msaa") || n.Contains("antialias") || n.Contains("multisampl"))
    {
        string v;
        try { v = p.GetValue(sv) == null ? "null" : p.GetValue(sv).ToString(); } catch { v = "<err>"; }
        sb.AppendLine("  " + p.Name + " : " + p.PropertyType.Name + " = " + v);
    }
}
sb.AppendLine("--- EditorPrefs keys mentioning scene/aa/msaa ---");
var ek = typeof(UnityEditor.EditorPrefs);
var m = ek.GetMethod("GetFloat", new System.Type[] { typeof(string), typeof(float) });
sb.AppendLine("(EditorPrefs are not enumerable; probing likely keys)");
foreach (var key in new string[] { "SceneViewMSAA", "SceneViewAntiAliasing", "kSceneViewMSAA", "SceneViewAA", "AntiAliasing" })
{
    string v;
    try { var mm = ek.GetMethod("GetInt", new System.Type[] { typeof(string), typeof(int) }); v = mm.Invoke(null, new object[] { key, -999 }).ToString(); }
    catch { v = "<err>"; }
    sb.AppendLine("  " + key + " = " + v);
}
return sb.ToString();
