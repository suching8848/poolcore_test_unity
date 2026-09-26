var cam=UnityEngine.Camera.main;
var p=cam.transform.position;var q=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(1600,900,24);
System.IO.Directory.CreateDirectory("artifacts/water");
try {
cam.transform.position=new UnityEngine.Vector3(25,1.65f,2.6f);cam.transform.LookAt(new UnityEngine.Vector3(25,.8f,10));cam.targetTexture=rt;cam.Render();
foreach(var c in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Camera>()) {
if(!c.name.Contains("Water Reflection Camera"))continue;
var owner=UnityEngine.GameObject.Find(c.name.Replace(" Reflection Camera",""));
var block=new UnityEngine.MaterialPropertyBlock();owner.GetComponent<UnityEngine.Renderer>().GetPropertyBlock(block);
var tex=block.GetTexture("_PlanarReflection") as UnityEngine.RenderTexture;
if(!tex)continue;
UnityEngine.RenderTexture.active=tex;var read=new UnityEngine.Texture2D(tex.width,tex.height,UnityEngine.TextureFormat.RGB24,false);read.ReadPixels(new UnityEngine.Rect(0,0,tex.width,tex.height),0,0);read.Apply();System.IO.File.WriteAllBytes("artifacts/water/"+owner.name+"-reflection.png",read.EncodeToPNG());UnityEngine.Object.DestroyImmediate(read);
}
}finally{cam.transform.SetPositionAndRotation(p,q);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
return "captured reflections";
