using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Poolcore.Editor
{
    public static class PoolRimRepair
    {
        public struct Part { public string name; public Vector3 position, size; }

        // The old 16 cm wall straddled the opening, leaving 8 cm under the deck.
        // Keep its visible 8 cm, entirely inside the opening. Partition corners and
        // the stair entrance as well: even equal-material coplanar faces can flicker.
        public static IEnumerable<Part> Layout(string name,float x0,float x1,float z0,float z1,float bottom,bool steps)
        {
            const float t=.08f;
            Part Box(string suffix,float left,float right,float near,float far) => new Part {
                name=name+" Rim "+suffix,
                position=new Vector3((left+right)/2,bottom/2,(near+far)/2),
                size=new Vector3(right-left,-bottom,far-near)
            };
            yield return Box("West",x0,x0+t,z0,z1);
            yield return Box("East",x1-t,x1,z0,z1);
            yield return Box("North",x0+t,x1-t,z1-t,z1);
            if(steps) {
                float middle=(x0+x1)/2;
                yield return Box("South Left",x0+t,middle-1.5f,z0,z0+t);
                yield return Box("South Right",middle+1.5f,x1-t,z0,z0+t);
            } else yield return Box("South",x0+t,x1-t,z0,z0+t);
        }

        [MenuItem("Poolcore/Repair Pool Rim Overlaps")]
        public static void Repair()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying || scene.path!=PoolroomsBuilder.ScenePath)
                throw new InvalidOperationException("Open Poolrooms outside Play Mode first.");
            var zones=UnityEngine.Object.FindObjectsByType<WaterZone>();
            var jobs=new List<(MeshRenderer[] walls, WaterZone zone, string name)>();
            foreach(var zone in zones) {
                string name=zone.name.Replace(" Water","");
                var walls=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r =>
                    r.gameObject.scene==scene && new[]{" West"," East"," North"," South"}.Any(s=>r.name==name+s)
                    && Mathf.Abs(r.bounds.max.y)<.001f && Mathf.Abs(r.bounds.size.y-.9f)<.001f).ToArray();
                if(walls.Length!=4) throw new InvalidOperationException(name+": expected exactly four original walls, found "+walls.Length);
                jobs.Add((walls,zone,name));
            }
            if(jobs.Count!=3) throw new InvalidOperationException("Expected three pools.");
            Directory.CreateDirectory("artifacts/rim-fix");
            File.Copy(scene.path,"artifacts/rim-fix/Poolrooms-before-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Repair pool rim overlaps");
            foreach(var job in jobs) {
                var source=job.walls[0];var parent=source.transform.parent;
                var material=source.sharedMaterial;var flags=GameObjectUtility.GetStaticEditorFlags(source.gameObject);
                foreach(var part in Layout(job.name,job.zone.min.x,job.zone.max.x,job.zone.min.y,job.zone.max.y,-.9f,true)) {
                    var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Undo.RegisterCreatedObjectUndo(go,"Create inset rim");
                    go.name=part.name;go.transform.SetParent(parent);go.transform.position=part.position;go.transform.localScale=part.size;
                    go.GetComponent<Renderer>().sharedMaterial=material;GameObjectUtility.SetStaticEditorFlags(go,flags);
                }
                foreach(var wall in job.walls) Undo.DestroyObjectImmediate(wall.gameObject);
            }
            Undo.CollapseUndoOperations(group);
            Physics.SyncTransforms();
            string result=Validate();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Debug.Log(result);
        }

        public static string Validate()
        {
            var all=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r=>r.enabled && r.sharedMaterial && r.sharedMaterial.renderQueue<3000).ToArray();
            var rims=all.Where(r=>r.name.Contains(" Rim ")).ToArray();
            var failures=new List<string>();
            foreach(var rim in rims) foreach(var other in all) {
                if(rim==other) continue;
                var a=rim.bounds;var b=other.bounds;
                if(Mathf.Abs(a.max.y-b.max.y)>.0001f) continue;
                float x=Mathf.Min(a.max.x,b.max.x)-Mathf.Max(a.min.x,b.min.x);
                float z=Mathf.Min(a.max.z,b.max.z)-Mathf.Max(a.min.z,b.min.z);
                if(x>.0001f && z>.0001f) failures.Add(rim.name+" overlaps "+other.name+" by "+x*z+" m2");
            }
            if(rims.Length!=15) failures.Add("Expected 15 rim segments, found "+rims.Length);
            string result="Pool rim top-face validation: "+rims.Length+" segments; "+failures.Count+" overlaps/errors.\n"+string.Join("\n",failures);
            Directory.CreateDirectory("artifacts/rim-fix");File.WriteAllText("artifacts/rim-fix/geometry-validation.txt",result);
            if(failures.Count>0) throw new InvalidOperationException(result);
            return result;
        }
    }
}
