var type=System.Type.GetType("Poolcore.RoomAcoustics, Assembly-CSharp");var method=type.GetMethod("Profile");
float maxStep=0;var previous=UnityEngine.Vector2.zero;
for(int i=0;i<=900;i++) { var p=(UnityEngine.Vector2)method.Invoke(null,new object[]{new UnityEngine.Vector3(32+i*.01f,1.7f,22)});if(i>0)maxStep=UnityEngine.Mathf.Max(maxStep,UnityEngine.Mathf.Abs(p.x-previous.x));previous=p; }
var report=new {maximumDecayChangePerCentimetre=maxStep,hall=method.Invoke(null,new object[]{new UnityEngine.Vector3(58,1.7f,16)}),recess=method.Invoke(null,new object[]{new UnityEngine.Vector3(36,1.7f,22)}),quiet=method.Invoke(null,new object[]{new UnityEngine.Vector3(-3,1.7f,26)}),worldLabels=UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Length,lightmaps=UnityEngine.LightmapSettings.lightmaps.Length};
if(maxStep>.03f)throw new System.Exception("Acoustics spatial discontinuity");return report;
