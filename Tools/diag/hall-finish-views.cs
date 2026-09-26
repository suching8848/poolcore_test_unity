var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(1600,900,24);var image=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);
var points=new[]{new UnityEngine.Vector3(58,1.65f,0),new UnityEngine.Vector3(74,1.65f,22),new UnityEngine.Vector3(36,1.65f,22),new UnityEngine.Vector3(42.3f,1.65f,22),new UnityEngine.Vector3(39,1.65f,28),new UnityEngine.Vector3(43,1.65f,9)};
var looks=new[]{new UnityEngine.Vector3(58,.3f,25),new UnityEngine.Vector3(38,1.5f,22),new UnityEngine.Vector3(66,1.2f,20),new UnityEngine.Vector3(34,1.6f,22),new UnityEngine.Vector3(39,1.6f,15),new UnityEngine.Vector3(44,0,11)};
try{for(int i=0;i<points.Length;i++){cam.transform.SetPositionAndRotation(points[i],UnityEngine.Quaternion.LookRotation(looks[i]-points[i]));cam.targetTexture=rt;cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);image.Apply();System.IO.File.WriteAllBytes("artifacts/hall-finish/view-"+i+".png",image.EncodeToPNG());}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
return "Captured 6 depth hall views";

