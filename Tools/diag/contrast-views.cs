var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(1600,900,24);var image=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);
var points=new[]{new UnityEngine.Vector3(40,1.7f,42),new UnityEngine.Vector3(40.5f,1.7f,46),new UnityEngine.Vector3(40.5f,1.7f,57),new UnityEngine.Vector3(40.5f,1.7f,60),new UnityEngine.Vector3(65.5f,1.7f,90),new UnityEngine.Vector3(50,1.7f,61),new UnityEngine.Vector3(67,1.7f,85)};
var looks=new[]{new UnityEngine.Vector3(43.5f,1.8f,43),new UnityEngine.Vector3(40.5f,1.7f,64),new UnityEngine.Vector3(43,2,76),new UnityEngine.Vector3(53,3,79),new UnityEngine.Vector3(44,3,61),new UnityEngine.Vector3(30,1,61),new UnityEngine.Vector3(80,1.7f,93)};
try{for(int i=0;i<points.Length;i++){cam.transform.SetPositionAndRotation(points[i],UnityEngine.Quaternion.LookRotation(looks[i]-points[i]));cam.targetTexture=rt;cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);image.Apply();System.IO.File.WriteAllBytes("artifacts/contrast-wing/view-"+i+".png",image.EncodeToPNG());}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
return "Captured seven contrast-route views";

