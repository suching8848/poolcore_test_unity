// Isolate depth ownership inside the overlap, without lighting, water, or AA.
var cam=UnityEngine.Camera.main;
var data=cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
var pos=cam.transform.position; var rot=cam.transform.rotation; var target=cam.targetTexture;
var active=UnityEngine.RenderTexture.active; var aa=data.antialiasing; var post=data.renderPostProcessing;
var renderers=UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>();
var originals=new System.Collections.Generic.Dictionary<UnityEngine.Renderer,UnityEngine.Material>();
var enabled=new System.Collections.Generic.Dictionary<UnityEngine.Renderer,bool>();
UnityEngine.Transform wall=null;
var red=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit")); red.SetColor("_BaseColor",UnityEngine.Color.red);
var blue=new UnityEngine.Material(red); blue.SetColor("_BaseColor",UnityEngine.Color.blue);
foreach(var r in renderers) {
 if(r.sharedMaterial && r.sharedMaterial.shader.name=="Poolcore/Still Water") {enabled[r]=r.enabled;r.enabled=false;}
 if((r.name=="Atrium East" || r.name=="Atrium Rim East") && UnityEngine.Mathf.Abs(r.bounds.max.y)<0.001f) {
  originals[r]=r.sharedMaterial;
  if(r.bounds.size.y>0.8f && r.bounds.size.x<0.2f) {wall=r.transform;r.sharedMaterial=blue;}
  else r.sharedMaterial=red;
 }
}
if(wall==null) throw new System.Exception("Pool wall not found");
var wp=wall.position;var ws=wall.localScale;
bool repaired=wall.name.Contains(" Rim ");
var rt=new UnityEngine.RenderTexture(1000,800,24);
var tex=new UnityEngine.Texture2D(1000,800,UnityEngine.TextureFormat.RGB24,false);
var sb=new System.Text.StringBuilder();
try {
 data.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.None;data.renderPostProcessing=false;cam.targetTexture=rt;
 for(int mode=0;mode<2;mode++) {
  if(mode==1) {wall.position=new UnityEngine.Vector3(6.96f,wp.y,wp.z);wall.localScale=new UnityEngine.Vector3(.08f,ws.y,ws.z);}
  int redCount=0,blueCount=0,flips=0;var previous=new int[100];
  for(int frame=0;frame<41;frame++) {
   cam.transform.position=new UnityEngine.Vector3(9,1.8f,0);
   cam.transform.rotation=UnityEngine.Quaternion.LookRotation(new UnityEngine.Vector3(7,0,3.5f)-cam.transform.position)*UnityEngine.Quaternion.Euler(0,-10+frame*.5f,0);
   cam.Render();UnityEngine.RenderTexture.active=rt;tex.ReadPixels(new UnityEngine.Rect(0,0,1000,800),0,0);tex.Apply();
   for(int n=0;n<100;n++) {
    var screen=cam.WorldToScreenPoint(new UnityEngine.Vector3(7.04f,0,1.5f+n*.035f));
    if(screen.x<1 || screen.x>998 || screen.y<1 || screen.y>798) continue;
    var c=tex.GetPixel((int)screen.x,(int)screen.y);int owner=c.r>c.b?1:2;
    if(owner==1) redCount++;else blueCount++;
    if(frame>0 && previous[n]!=owner) flips++;previous[n]=owner;
   }
   if(frame==20) System.IO.File.WriteAllBytes("artifacts/diag/depth-proof-"+(repaired?"repaired-":"")+mode+".png",tex.EncodeToPNG());
  }
  sb.AppendLine((mode==0?(repaired?"REPAIRED":"BEFORE"):"INSET")+" deck samples="+redCount+" pool-wall samples="+blueCount+" ownership flips="+flips);
 }
} finally {
 wall.position=wp;wall.localScale=ws;
 foreach(var kv in originals) kv.Key.sharedMaterial=kv.Value;foreach(var kv in enabled) kv.Key.enabled=kv.Value;
 cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=target;data.antialiasing=aa;data.renderPostProcessing=post;UnityEngine.RenderTexture.active=active;
 rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(red);UnityEngine.Object.DestroyImmediate(blue);
}
System.IO.File.WriteAllText("artifacts/diag/depth-proof"+(repaired?"-repaired":"")+".txt",sb.ToString());return sb.ToString();
