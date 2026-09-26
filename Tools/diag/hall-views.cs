var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(1600,900,24);var image=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);
var points=new[]{new UnityEngine.Vector3(58,1.65f,0),new UnityEngine.Vector3(42.3f,1.65f,16),new UnityEngine.Vector3(58,1.65f,33),new UnityEngine.Vector3(58,.9f,7)};
var looks=new[]{new UnityEngine.Vector3(58,.3f,25),new UnityEngine.Vector3(61,-.3f,16),new UnityEngine.Vector3(58,-.3f,12),new UnityEngine.Vector3(60,.2f,26)};
try{for(int i=0;i<points.Length;i++){cam.transform.SetPositionAndRotation(points[i],UnityEngine.Quaternion.LookRotation(looks[i]-points[i]));cam.targetTexture=rt;cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);image.Apply();System.IO.File.WriteAllBytes("artifacts/depth-hall/view-"+i+".png",image.EncodeToPNG());}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
return "Captured 4 depth hall views";
