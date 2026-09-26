if(!UnityEngine.Application.isPlaying)return "Play Mode required";
var player=UnityEngine.Object.FindAnyObjectByType<Poolcore.FirstPersonController>();player.Release();player.Teleport(new UnityEngine.Vector3(19.7f,.05f,2.7f));player.transform.rotation=UnityEngine.Quaternion.identity;
var points=new[]{new UnityEngine.Vector2(29.8f,2.7f),new UnityEngine.Vector2(29.8f,8.8f),new UnityEngine.Vector2(42.3f,8.8f),new UnityEngine.Vector2(42.3f,32),new UnityEngine.Vector2(74,32),new UnityEngine.Vector2(74,0),new UnityEngine.Vector2(58,0),new UnityEngine.Vector2(58,7),new UnityEngine.Vector2(58,10.8f),new UnityEngine.Vector2(58,0),new UnityEngine.Vector2(42.3f,0),new UnityEngine.Vector2(42.3f,8.8f),new UnityEngine.Vector2(29.8f,8.8f),new UnityEngine.Vector2(29.8f,2.7f),new UnityEngine.Vector2(19.7f,2.7f)};
var sb=new System.Text.StringBuilder();int failures=0;
try {
foreach(var target in points){int i=0;for(;i<2400;i++){var delta=target-new UnityEngine.Vector2(player.transform.position.x,player.transform.position.z);if(delta.magnitude<.12f)break;player.SimulateMovement(delta.normalized,false,1f/60);}
bool pass=i<2400;sb.AppendLine((pass?"PASS":"FAIL")+" route "+target+" at "+player.transform.position);if(!pass){failures++;break;}}
player.Teleport(new UnityEngine.Vector3(58,-.65f,10));for(int i=0;i<300;i++)player.SimulateMovement(UnityEngine.Vector2.up,false,1f/60);
bool blocked=player.transform.position.z<12 && player.transform.position.z>11 && player.transform.position.y>-1.3f;
sb.AppendLine((blocked?"PASS":"FAIL")+" deep-water boundary at "+player.transform.position);if(!blocked)failures++;
}finally{player.ResetToSpawn();}
sb.AppendLine("Failures: "+failures);System.IO.File.WriteAllText("artifacts/depth-hall/route.txt",sb.ToString());return sb.ToString();

