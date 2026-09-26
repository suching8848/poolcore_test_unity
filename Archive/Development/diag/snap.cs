// Fixed-camera temporal snapshot. Call repeatedly: the editor ticks between calls, so
// _Time advances while the camera pose stays put. Diffing two snapshots shows exactly
// which pixels are temporally unstable (animated) while everything else is identical.
var cam = UnityEngine.Camera.main;
const int W = 1600, H = 900;
string dir = @"E:\code\test_astra\Poolcore\Poolcore\artifacts\diag";
System.IO.Directory.CreateDirectory(dir);
var rt = new UnityEngine.RenderTexture(W, H, 24);
var sp = cam.transform.position; var sr = cam.transform.rotation; var st = cam.targetTexture; var sa = UnityEngine.RenderTexture.active;
string path;
try
{
    // Deck south of the atrium pool, looking across the near waterline.
    cam.transform.position = new UnityEngine.Vector3(0f, 1.5f, -5.5f);
    cam.transform.LookAt(new UnityEngine.Vector3(0f, -0.1f, 6f));
    cam.targetTexture = rt; cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
    var t = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
    t.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); t.Apply();
    path = System.IO.Path.Combine(dir, "anim-f" + UnityEngine.Time.frameCount + ".png");
    System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(t);
    UnityEngine.RenderTexture.active = prev;
}
finally
{
    cam.transform.position = sp; cam.transform.rotation = sr; cam.targetTexture = st; UnityEngine.RenderTexture.active = sa;
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
}
return "frame=" + UnityEngine.Time.frameCount + " time=" + UnityEngine.Time.time.ToString("F3") + " -> " + path;
