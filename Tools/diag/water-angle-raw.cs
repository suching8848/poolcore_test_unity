var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(960,540,24);var image=new UnityEngine.Texture2D(960,540,UnityEngine.TextureFormat.RGB24,false);
System.IO.Directory.CreateDirectory("artifacts/water-angle");
try{foreach(float pitch in new[]{30f,15f,5f,0f,-5f,-15f,-25f}){cam.transform.SetPositionAndRotation(new UnityEngine.Vector3(58.37f,.59f,11.63f),UnityEngine.Quaternion.Euler(pitch,0,0));cam.targetTexture=rt;cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,960,540),0,0);image.Apply();System.IO.File.WriteAllBytes("artifacts/water-angle/wading-before-"+pitch+".png",image.EncodeToPNG());var water=UnityEngine.GameObject.Find("Depth Hall Water").GetComponent<Poolcore.PlanarWaterReflection>();var tex=water.ReflectionTexture;UnityEngine.RenderTexture.active=tex;var read=new UnityEngine.Texture2D(tex.width,tex.height,UnityEngine.TextureFormat.RGB24,false);read.ReadPixels(new UnityEngine.Rect(0,0,tex.width,tex.height),0,0);read.Apply();System.IO.File.WriteAllBytes("artifacts/water-angle/raw-"+pitch+".png",read.EncodeToPNG());UnityEngine.Object.DestroyImmediate(read);}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
return "Captured angle sweep";


