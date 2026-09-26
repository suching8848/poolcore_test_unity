UnityEngine.Physics.SyncTransforms();
var report=new System.Text.StringBuilder();int failures=0;
foreach(var r in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>()) {
if(!r.name.Contains("Pillar") || r.name.Contains("Plinth") || r.name.Contains("Foot"))continue;
var b=r.bounds;bool overlap=false;
foreach(var w in UnityEngine.Object.FindObjectsByType<UnityEngine.Renderer>()) {
if(!w.sharedMaterial || w.sharedMaterial.shader.name!="Poolcore/Still Water")continue;var water=w.bounds;
if(b.min.x<water.max.x && b.max.x>water.min.x && b.min.z<water.max.z && b.max.z>water.min.z && b.min.y>water.center.y)overlap=true;
}
if(overlap) {report.AppendLine("FAIL unsupported pool pillar "+r.name+" "+b.center);failures++;}
}
report.AppendLine("Pillars over pool openings: "+failures);
foreach(float z in new[]{7f,15f,25f}) {
UnityEngine.RaycastHit hit;
if(UnityEngine.Physics.Raycast(new UnityEngine.Vector3(58,2,z),UnityEngine.Vector3.down,out hit,10)) report.AppendLine("z="+z+" bottom="+hit.point.y+" depth="+(-.28f-hit.point.y)+" slope="+UnityEngine.Vector3.Angle(hit.normal,UnityEngine.Vector3.up));
else {report.AppendLine("FAIL missing pool floor "+z);failures++;}
}
report.AppendLine("Failures: "+failures);System.IO.File.WriteAllText("artifacts/depth-hall/geometry.txt",report.ToString());return report.ToString();
