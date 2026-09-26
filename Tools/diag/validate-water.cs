var camera=UnityEngine.Camera.main;var pos=camera.transform.position;var rot=camera.transform.rotation;var target=camera.targetTexture;var active=UnityEngine.RenderTexture.active;
var data=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();var aa=data.antialiasing;
var marker=UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);marker.name="Temporary reflection verification marker";
var material=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new UnityEngine.Color(1,0,1));
marker.GetComponent<UnityEngine.Renderer>().sharedMaterial=material;marker.transform.position=new UnityEngine.Vector3(-8.4f,1.3f,4);marker.transform.localScale=UnityEngine.Vector3.one;
var rt=new UnityEngine.RenderTexture(1600,900,24);var read=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);var report=new System.Text.StringBuilder();
try {
 camera.transform.position=new UnityEngine.Vector3(9,1.65f,5);camera.transform.LookAt(new UnityEngine.Vector3(-4,0,4));camera.targetTexture=rt;data.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.None;
 System.Func<UnityEngine.Color[]> capture=()=>{camera.Render();UnityEngine.RenderTexture.active=rt;read.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);read.Apply();return read.GetPixels();};
 var withMarker=capture();System.IO.File.WriteAllBytes("artifacts/water/marker-reflection.png",read.EncodeToPNG());
 marker.SetActive(false);var withoutMarker=capture();
 var predicted=camera.WorldToScreenPoint(new UnityEngine.Vector3(-8.4f,2*(-.28f)-1.3f,4));
 int changed=0;
 for(int y=(int)predicted.y-12;y<=(int)predicted.y+12;y++)for(int x=(int)predicted.x-12;x<=(int)predicted.x+12;x++) {
 if(x<0||x>=1600||y<0||y>=900)continue;var a=withMarker[y*1600+x];var b=withoutMarker[y*1600+x];if(UnityEngine.Mathf.Abs(a.r-b.r)+UnityEngine.Mathf.Abs(a.b-b.b)>.04f)changed++;
 }
 report.AppendLine((changed>20?"PASS":"FAIL")+" reflected marker at predicted pixel "+predicted+" changed="+changed);
 var still=capture();float delta=0;for(int i=0;i<still.Length;i++)delta+=UnityEngine.Mathf.Abs(still[i].r-withoutMarker[i].r)+UnityEngine.Mathf.Abs(still[i].g-withoutMarker[i].g)+UnityEngine.Mathf.Abs(still[i].b-withoutMarker[i].b);
 delta/=still.Length*3;report.AppendLine((delta<.002f?"PASS":"FAIL")+" fixed-view frame difference="+delta);
 System.IO.File.WriteAllBytes("artifacts/water/atrium-calm.png",read.EncodeToPNG());
}finally{camera.transform.SetPositionAndRotation(pos,rot);camera.targetTexture=target;data.antialiasing=aa;UnityEngine.RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(marker);UnityEngine.Object.DestroyImmediate(material);rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(read);}
System.IO.File.WriteAllText("artifacts/water/reflection-validation.txt",report.ToString());return report.ToString();
