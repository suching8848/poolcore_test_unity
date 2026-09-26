using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Poolcore.Editor
{
    public static class LargeHallBuilder
    {
        private static Transform root;
        private static Material wall,deck,tile,trim;
        private static GameObject Box(string name,Vector3 p,Vector3 size,Material material)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;
            g.GetComponent<Renderer>().sharedMaterial=material;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);
            return g;
        }
        private static void Floor(string name,float x0,float x1,float z0,float z1) => Box(name,new Vector3((x0+x1)/2,-.2f,(z0+z1)/2),new Vector3(x1-x0,.4f,z1-z0),deck);
        private static Material Material(string name,Color color,float scale,float smoothness)
        {
            string path="Assets/Poolcore/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m) {m=new Material(Shader.Find("Poolcore/Porcelain"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetColor("_Grout",color*.62f);m.SetFloat("_TileSize",scale);m.SetFloat("_Smoothness",smoothness);EditorUtility.SetDirty(m);return m;
        }
        [MenuItem("Poolcore/Add Large Depth Hall")]
        public static void Add()
        {
            if(Application.isPlaying || EditorSceneManager.GetActiveScene().path!=PoolroomsBuilder.ScenePath) throw new InvalidOperationException("Open Poolrooms outside Play Mode.");
            Directory.CreateDirectory("artifacts/depth-hall");
            File.Copy(PoolroomsBuilder.ScenePath,"artifacts/depth-hall/Poolrooms-before-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            AddToScene();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        public static void AddToScene()
        {
            ClearWarmEntry();
            if(GameObject.Find("Depth Hall Architecture")) return;
            // These columns used to sit over the Quiet pool, with their bases at deck height.
            foreach(var p in UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r=>r.name=="Quiet Pillar")) {
                var v=p.transform.position;v.x=v.x<0?-9:2;p.transform.position=v;
            }
            var east=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Single(r=>r.name=="Warm East" && Mathf.Abs(r.transform.position.x-32)<.01f && r.bounds.size.y>4);
            var oldParent=east.transform.parent;var oldMaterial=east.sharedMaterial;
            UnityEngine.Object.DestroyImmediate(east.gameObject);
            root=oldParent;
            Box("Warm East South",new Vector3(32,2.4f,4.5f),new Vector3(.35f,4.8f,7),oldMaterial);
            Box("Warm East North",new Vector3(32,2.4f,13.5f),new Vector3(.35f,4.8f,5),oldMaterial);
            Box("Warm East Lintel",new Vector3(32,4.1f,9.5f),new Vector3(.35f,1.4f,3),oldMaterial);
            root=new GameObject("Depth Hall Architecture").transform;
            wall=Material("Depth Hall Ivory",new Color(.79f,.83f,.8f),.65f,.4f);
            deck=Material("Depth Hall Deck",new Color(.71f,.78f,.76f),.6f,.48f);
            tile=Material("Depth Hall Pool Tile",new Color(.38f,.67f,.68f),.4f,.38f);
            trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/Deep Green Trim.mat");
            Floor("Hall Connector",32,38,8,11);
            Box("Hall Connector South",new Vector3(35,1.75f,8),new Vector3(6,3.5f,.25f),wall);
            Box("Hall Connector North",new Vector3(35,1.75f,11),new Vector3(6,3.5f,.25f),wall);
            Box("Hall Connector Roof",new Vector3(35,3.6f,9.5f),new Vector3(6,.2f,3),wall);
            Floor("Hall South Concourse",38,78,-4,2);Floor("Hall North Concourse",38,78,30,36);
            Floor("Hall West Concourse",38,44,2,30);Floor("Hall East Concourse",72,78,2,30);
            Box("Hall West South",new Vector3(38,5.5f,2),new Vector3(.35f,11,12),wall);
            Box("Hall West North",new Vector3(38,5.5f,23.5f),new Vector3(.35f,11,25),wall);
            Box("Hall Entry Lintel",new Vector3(38,7.25f,9.5f),new Vector3(.35f,7.5f,3),wall);
            Box("Hall East Wall",new Vector3(78,5.5f,16),new Vector3(.35f,11,40),wall);
            Box("Hall South Wall",new Vector3(58,5.5f,-4),new Vector3(40,11,.35f),wall);
            Box("Hall North Wall",new Vector3(58,5.5f,36),new Vector3(40,11,.35f),wall);
            BasinSection("Entry Slope",2,5,0,-.65f);BasinSection("Shallow Shelf",5,10,-.65f,-.65f);
            BasinSection("Depth Transition",10,20,-.65f,-3.4f);BasinSection("Deep Basin",20,30,-3.4f,-3.4f);
            Box("Hall Pool West Rim",new Vector3(44.04f,-1.7f,16),new Vector3(.08f,3.4f,28),tile);
            Box("Hall Pool East Rim",new Vector3(71.96f,-1.7f,16),new Vector3(.08f,3.4f,28),tile);
            Box("Hall Pool North Rim",new Vector3(58,-1.7f,29.96f),new Vector3(27.84f,3.4f,.08f),tile);
            var water=GameObject.CreatePrimitive(PrimitiveType.Quad);water.name="Depth Hall Water";water.transform.SetParent(root);
            water.transform.position=new Vector3(58,-.28f,16);water.transform.rotation=Quaternion.Euler(90,0,0);water.transform.localScale=new Vector3(27.84f,28,1);
            UnityEngine.Object.DestroyImmediate(water.GetComponent<Collider>());
            var wm=AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/Depth Hall Water.mat");
            if(!wm) {wm=new Material(Shader.Find("Poolcore/Still Water"));AssetDatabase.CreateAsset(wm,"Assets/Poolcore/Materials/Depth Hall Water.mat");}water.GetComponent<Renderer>().sharedMaterial=wm;
            CalmWaterSetup.Configure(water);
            var zone=water.AddComponent<WaterZone>();zone.min=new Vector2(44,2);zone.max=new Vector2(72,30);zone.surface=-.28f;
            foreach(float x in new[]{40.5f,75.5f}) foreach(float z in new[]{2f,10f,18f,26f,34f}) {
                Box("Hall Pillar",new Vector3(x,5.5f,z),new Vector3(1,11,1),wall);
                Box("Hall Pillar Plinth",new Vector3(x,.1f,z),new Vector3(1.4f,.2f,1.4f),trim);
            }
            foreach(var span in new[]{new Vector2(38,49),new Vector2(55,65),new Vector2(71,78)})
                Box("Hall Roof",new Vector3((span.x+span.y)/2,11.1f,16),new Vector3(span.y-span.x,.3f,40),wall);
            foreach(float z in new[]{-4f,6f,16f,26f,36f}) Box("Hall Roof Beam",new Vector3(58,10.7f,z),new Vector3(40,.6f,.55f),wall);
            foreach(var p in new[]{new Vector3(50,8,10),new Vector3(66,8,25)}) {
                var g=new GameObject("Hall Soft Fill");g.transform.SetParent(root);g.transform.position=p;
                var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(.77f,.9f,1);light.intensity=36;light.range=30;light.bounceIntensity=2;light.shadows=LightShadows.None;
            }
            // A visible pool safety rope marks the maximum wading depth. The deep end
            // remains an observation space until swimming is implemented.
            var rope=Material("Depth Hall Safety Rope",new Color(.83f,.72f,.42f),3,.25f);
            Box("Depth Boundary Rope",new Vector3(58,-.24f,12),new Vector3(27.84f,.045f,.045f),rope);
            for(int i=0;i<19;i++) Box("Depth Boundary Float",new Vector3(44.5f+i*1.5f,-.23f,12),new Vector3(.22f,.12f,.16f),i%2==0?wall:rope);
            var barrier=new GameObject("Deep Water Wading Limit");barrier.transform.SetParent(root);barrier.transform.position=new Vector3(58,0,12);
            barrier.AddComponent<BoxCollider>().size=new Vector3(28,3,.1f);
            foreach(float x in new[]{49f,65f}) {
                Box("Hall Viewing Bench",new Vector3(x,.45f,33.5f),new Vector3(6,.18f,.8f),wall);
                foreach(float side in new[]{-2f,2f}) Box("Hall Bench Support",new Vector3(x+side,.18f,33.5f),new Vector3(.25f,.36f,.65f),wall);
            }
            Physics.SyncTransforms();
        }
        private static void ClearWarmEntry()
        {
            var bench=GameObject.Find("Warm Bench");if(!bench) return;
            root=bench.transform.parent;var material=bench.GetComponent<Renderer>().sharedMaterial;
            bench.transform.position=new Vector3(30.9f,.45f,5.9f);bench.transform.localScale=new Vector3(.8f,.25f,1.8f);
            if(!GameObject.Find("Warm Entry North Bench")) {
                Box("Warm Entry North Bench",new Vector3(30.9f,.45f,13),new Vector3(.8f,.25f,2),material);
                foreach(float z in new[]{5.3f,6.5f,12.3f,13.7f}) Box("Warm Entry Bench Support",new Vector3(30.9f,.1625f,z),new Vector3(.55f,.325f,.22f),material);
            }
        }
        private static void BasinSection(string name,float z0,float z1,float y0,float y1)
        {
            Directory.CreateDirectory("Assets/Poolcore/Meshes");
            var mesh=new Mesh {name="Hall "+name};
            mesh.vertices=new[]{new Vector3(44,y0,z0),new Vector3(72,y0,z0),new Vector3(72,y1,z1),new Vector3(44,y1,z1),new Vector3(44,-3.8f,z0),new Vector3(72,-3.8f,z0),new Vector3(72,-3.8f,z1),new Vector3(44,-3.8f,z1)};
            mesh.triangles=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
            var vertices=mesh.vertices;var indices=mesh.triangles;
            mesh.vertices=indices.Select(i=>vertices[i]).ToArray();mesh.triangles=Enumerable.Range(0,indices.Length).ToArray();
            mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);
            string path="Assets/Poolcore/Meshes/Hall "+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing) {EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
            var serialized=new SerializedObject(mesh);
            serialized.FindProperty("m_PreBakeTriangleCollisionMesh").boolValue=true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var g=new GameObject("Hall "+name);g.transform.SetParent(root);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=tile;g.AddComponent<MeshCollider>().sharedMesh=mesh;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);
        }
    }
}
