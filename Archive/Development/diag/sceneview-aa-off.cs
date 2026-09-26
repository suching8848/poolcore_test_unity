// Turn off the Scene view's own anti-aliasing so URP stops disabling TAA for the Scene view.
// Unity exposes no public setter in older versions, so drive it through reflection.
var sb = new System.Text.StringBuilder();
var sv = UnityEditor.SceneView.lastActiveSceneView;
if (sv == null) return "ERROR: no lastActiveSceneView";
var t = typeof(UnityEditor.SceneView);
var bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var p1 = t.GetProperty("antiAliasing", bf);
var p2 = t.GetProperty("antiAlias", bf);
sb.AppendLine("before: antiAliasing=" + (p1 != null ? p1.GetValue(sv).ToString() : "-")
            + "  antiAlias=" + (p2 != null ? p2.GetValue(sv).ToString() : "-"));
if (p1 != null && p1.CanWrite) p1.SetValue(sv, 0);
if (p2 != null && p2.CanWrite) p2.SetValue(sv, 0);
sb.AppendLine("after : antiAliasing=" + (p1 != null ? p1.GetValue(sv).ToString() : "-")
            + "  antiAlias=" + (p2 != null ? p2.GetValue(sv).ToString() : "-"));
sv.Repaint();
return sb.ToString();
