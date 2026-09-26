var oldFog=UnityEngine.RenderSettings.fog;UnityEngine.RenderSettings.fog=false;try{var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(960,540,24);var image=new UnityEngine.Texture2D(960,540,UnityEngine.TextureFormat.RGB24,false);
System.IO.Directory.CreateDirectory("artifacts/water-angle");
try{foreach(float pitch in new[]{30f,15f,5f,0f,-5f,-15f,-25f}){cam.transform.SetPositionAndRotation(new UnityEngine.Vector3(58.37f,.59f,11.63f),UnityEngine.Quaternion.Euler(pitch,0,0));cam.targetTexture=rt;cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,960,540),0,0);image.Apply();System.IO.File.WriteAllBytes("artifacts/water-angle/nofog-"+pitch+".png",image.EncodeToPNG());}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}


}finally{UnityEngine.RenderSettings.fog=oldFog;}return "fog comparison";
