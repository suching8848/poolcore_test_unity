var cam=UnityEngine.Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var fov=cam.fieldOfView;var aspect=cam.aspect;var target=cam.targetTexture;var active=UnityEngine.RenderTexture.active;
var rt=new UnityEngine.RenderTexture(960,540,24);var image=new UnityEngine.Texture2D(960,540,UnityEngine.TextureFormat.RGB24,false);var report=new System.Text.StringBuilder();int samples=0;float worst=0;
try{cam.targetTexture=rt;cam.aspect=960f/540;
foreach(float height in new[]{.59f,1.1f,1.65f})foreach(float lens in new[]{60f,75f,90f}){
cam.fieldOfView=lens;var position=new UnityEngine.Vector3(58.37f,height,11.63f);var mirroredWall=new UnityEngine.Vector3(53.4f,-5.06f,35.9f);float t=(-.28f-height)/(mirroredWall.y-height);var waterPoint=UnityEngine.Vector3.Lerp(position,mirroredWall,t);
var low=new UnityEngine.Color(1,1,1);var high=UnityEngine.Color.black;int visible=0;
foreach(float pitch in new[]{30f,20f,10f,0f,-5f,-10f,-15f,-20f,-25f}){
cam.transform.SetPositionAndRotation(position,UnityEngine.Quaternion.Euler(pitch,0,0));var uv=cam.WorldToViewportPoint(waterPoint);if(uv.x<.02f||uv.x>.98f||uv.y<.02f||uv.y>.98f)continue;
cam.Render();UnityEngine.RenderTexture.active=rt;image.ReadPixels(new UnityEngine.Rect(0,0,960,540),0,0);image.Apply();var c=UnityEngine.Color.clear;
for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)c+=image.GetPixel((int)(uv.x*960)+x,(int)(uv.y*540)+y)/9f;
low=new UnityEngine.Color(UnityEngine.Mathf.Min(low.r,c.r),UnityEngine.Mathf.Min(low.g,c.g),UnityEngine.Mathf.Min(low.b,c.b));high=new UnityEngine.Color(UnityEngine.Mathf.Max(high.r,c.r),UnityEngine.Mathf.Max(high.g,c.g),UnityEngine.Mathf.Max(high.b,c.b));visible++;samples++;
}
float range=UnityEngine.Mathf.Max(high.r-low.r,high.g-low.g,high.b-low.b);worst=UnityEngine.Mathf.Max(worst,range);report.AppendLine("height="+height+" FOV="+lens+" views="+visible+" RGB range="+range.ToString("F4"));
}}
finally{cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;cam.fieldOfView=fov;cam.aspect=aspect;UnityEngine.RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
report.AppendLine((worst<.10f&&samples>=50?"PASS":"FAIL")+" samples="+samples+" worst range="+worst);System.IO.File.WriteAllText("artifacts/water-angle/regression.txt",report.ToString());return report.ToString();
