using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Poolcore.Editor
{
    public static class ContrastWingBuilder
    {
        private static Transform root;
        private static Material white, floor, shaded, trim;
        [MenuItem("Poolcore/Add Window Contrast Wing")]
        public static void Apply()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(Application.isPlaying || scene.path!=PoolroomsBuilder.ScenePath)throw new InvalidOperationException("Open Poolrooms outside Play Mode.");
            Directory.CreateDirectory("artifacts/contrast-wing");
            File.Copy(scene.path,"artifacts/contrast-wing/Before-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity",true);
            Configure();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        }
        public static void Configure()
        {
            var hall=GameObject.Find("Depth Hall Architecture");if(!hall)return;
            var previous=GameObject.Find("Window Contrast Wing");if(previous)UnityEngine.Object.DestroyImmediate(previous);
            root=new GameObject("Window Contrast Wing").transform;
            white=Surface("Window Hall Milk Plaster",new Color(.92f,.905f,.865f),.01f,.18f);
            floor=Surface("Window Hall Ivory Tile",new Color(.84f,.855f,.83f),.24f,.28f);
            shaded=Surface("Window Room Warm Plaster",new Color(.64f,.64f,.57f),.015f,.16f);
            trim=Surface("Window Limestone",new Color(.86f,.84f,.76f),.07f,.25f);
            var old=GameObject.Find("Hall North Wall");if(old)UnityEngine.Object.DestroyImmediate(old);
            var oldMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/Depth Hall Ivory.mat");
            Box("North Link West",new Vector3(38.75f,5.5f,36),new Vector3(1.5f,11,.35f),oldMat);
            Box("North Link East",new Vector3(60.25f,5.5f,36),new Vector3(35.5f,11,.35f),oldMat);
            Box("North Link Header",new Vector3(41,6.8f,36),new Vector3(3,8.4f,.35f),oldMat);
            // A sheltered link is wide enough to return to the existing hall.
            Floor("Return Link",39.5f,42.5f,36,39);
            Box("Return Link West",new Vector3(39.5f,1.3f,37.5f),new Vector3(.25f,2.6f,3),shaded);
            Box("Return Link East",new Vector3(42.5f,1.3f,37.5f),new Vector3(.25f,2.6f,3),shaded);
            Box("Return Link Roof",new Vector3(41,2.7f,37.5f),new Vector3(3,.2f,3),shaded);
            // Six by six metres, with a low ceiling and an actual open side window.
            Floor("Window Room Floor",37.5f,43.5f,39,45);
            Box("Window Room West",new Vector3(37.5f,1.6f,42),new Vector3(.3f,3.2f,6),shaded);
            Box("Window Room South Left",new Vector3(38.5f,1.6f,39),new Vector3(2,3.2f,.3f),shaded);
            Box("Window Room South Right",new Vector3(43,1.6f,39),new Vector3(1,3.2f,.3f),shaded);
            Box("Window Room South Header",new Vector3(41,2.9f,39),new Vector3(3,.6f,.3f),shaded);
            Box("Window Room North Left",new Vector3(38.575f,1.6f,45),new Vector3(2.15f,3.2f,.3f),shaded);
            Box("Window Room North Right",new Vector3(42.425f,1.6f,45),new Vector3(2.15f,3.2f,.3f),shaded);
            Box("Window Room North Header",new Vector3(40.5f,2.85f,45),new Vector3(1.7f,.7f,.3f),shaded);
            Box("Window Room Ceiling",new Vector3(40.5f,3.3f,42),new Vector3(6,.2f,6),shaded);
            EastWindow("Small Side Window",43.5f,39,45,40,44,1.05f,2.85f,3.2f,shaded);
            Box("Small Window Sill",new Vector3(43.45f,1.02f,42),new Vector3(.55f,.1f,4.2f),trim);
            Box("Small Window Mullion",new Vector3(43.5f,1.95f,42),new Vector3(.3f,1.8f,.065f),trim);
            Box("Outside Window Terrace",new Vector3(49.25f,-.2f,42),new Vector3(11.5f,.4f,12),white);
            Box("Small Courtyard Back",new Vector3(55,2.5f,42),new Vector3(.4f,5,12),white);
            foreach(float z in new[]{37f,47.5f})Box("Small Courtyard Side",new Vector3(49.25f,2.5f,z),new Vector3(11.5f,5,.4f),white);
            // The narrow passage hides the room's full width until the final step.
            Floor("Narrow Passage Floor",39.65f,41.35f,45,58);
            Box("Narrow Passage West",new Vector3(39.65f,1.25f,51.5f),new Vector3(.22f,2.5f,13),shaded);
            foreach(float z in new[]{46.5f,51.5f,56.5f})Box("Passage East Pier",new Vector3(41.35f,1.25f,z),new Vector3(.22f,2.5f,3),shaded);
            foreach(float z in new[]{49f,54f}) {
                Box("Passage Window Sill",new Vector3(41.35f,1,z),new Vector3(.22f,2,2),shaded);
                Box("Passage Window Header",new Vector3(41.35f,2.45f,z),new Vector3(.22f,.1f,2),shaded);
            }
            Box("Narrow Passage Ceiling",new Vector3(40.5f,2.6f,51.5f),new Vector3(1.7f,.2f,13),shaded);
            // Main room: forty by thirty-six metres, twelve metres high.
            Floor("Milk Hall South Deck",30,70,58,65);Floor("Milk Hall North Deck",30,70,87,94);
            Floor("Milk Hall West Deck",30,36,65,87);Floor("Milk Hall East Deck",64,70,65,87);
            Box("Milk Hall West Wall",new Vector3(30,6,76),new Vector3(.4f,12,36),white);
            Box("Milk Hall North Wall",new Vector3(50,6,94),new Vector3(40,12,.4f),white);
            Box("Milk Hall Ceiling",new Vector3(50,12.15f,76),new Vector3(40,.3f,36),white);
            // South portal plus two full-height windows, with thick stone reveals.
            Box("Milk Portal Left",new Vector3(34.825f,6,58),new Vector3(9.65f,12,.4f),white);
            Box("Milk Portal Right",new Vector3(43.675f,6,58),new Vector3(4.65f,12,.4f),white);
            Box("Milk Portal Header",new Vector3(40.5f,7.25f,58),new Vector3(1.7f,9.5f,.4f),white);
            foreach(float x in new[]{50f,62f}) {
                Box("Milk South Window Sill Wall",new Vector3(x,.45f,58),new Vector3(8,.9f,.4f),white);
                Box("Milk South Window Header",new Vector3(x,11.25f,58),new Vector3(8,1.5f,.4f),white);
                Box("Milk South Window Sill",new Vector3(x,.91f,58),new Vector3(8.3f,.12f,.8f),trim);
                Box("Milk Window Mullion",new Vector3(x,5.7f,58),new Vector3(.12f,9.6f,.45f),white);
                // Collision-only glazing keeps openings physically bounded without fake opaque glass.
                var pane=new GameObject("Window Boundary");pane.transform.SetParent(root);pane.transform.position=new Vector3(x,5.7f,58);pane.AddComponent<BoxCollider>().size=new Vector3(8,9.6f,.1f);
            }
            foreach(float x in new[]{56f,68f})Box("Milk South Pier",new Vector3(x,6,58),new Vector3(4,12,.4f),white);
            EastWindow("Milk East Window",70,58,94,65,87,1.1f,10.5f,12,white);
            foreach(float z in new[]{70.5f,76f,81.5f})Box("Milk East Mullion",new Vector3(70,5.8f,z),new Vector3(.45f,9.4f,.14f),white);
            Box("Milk Exterior Terrace",new Vector3(60,-.2f,53),new Vector3(32,.4f,10),white);
            Box("Milk South Courtyard Screen",new Vector3(60,6.5f,48),new Vector3(32,13,.4f),white);
            Box("Milk East Terrace",new Vector3(77,-.2f,76),new Vector3(14,.4f,40),white);
            Box("Milk East Courtyard Screen",new Vector3(84,4,76),new Vector3(.4f,8,40),white);
            foreach(float x in new[]{44f,76f})Box("Milk South Courtyard End",new Vector3(x,6.5f,53),new Vector3(.4f,13,10),white);
            foreach(float z in new[]{56f,96f})Box("Milk East Courtyard End",new Vector3(77,4,z),new Vector3(14,8,.4f),white);
            // Pool edges, gently descending entry and a quiet uninterrupted water plane.
            var poolTile=Surface("Milk Pool Porcelain",new Color(.65f,.80f,.78f),.2f,.3f);
            Box("Milk Pool Bottom",new Vector3(50,-.9f,76),new Vector3(28,.4f,22),poolTile);
            foreach(float x in new[]{36.04f,63.96f})Box("Milk Pool Side",new Vector3(x,-.35f,76),new Vector3(.08f,.7f,22),poolTile);
            Box("Milk Pool North",new Vector3(50,-.35f,86.96f),new Vector3(28,.7f,.08f),poolTile);
            foreach(float x in new[]{41.5f,58.5f})Box("Milk Pool South",new Vector3(x,-.35f,65.04f),new Vector3(11,.7f,.08f),poolTile);
            Ramp(poolTile);
            var water=GameObject.CreatePrimitive(PrimitiveType.Quad);water.name="Milk Hall Water";water.transform.SetParent(root);
            water.transform.SetPositionAndRotation(new Vector3(50,-.12f,76),Quaternion.Euler(90,0,0));water.transform.localScale=new Vector3(27.84f,21.84f,1);UnityEngine.Object.DestroyImmediate(water.GetComponent<Collider>());
            const string wp="Assets/Poolcore/Materials/Milk Hall Water.mat";var wm=AssetDatabase.LoadAssetAtPath<Material>(wp);
            if(!wm){wm=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/Atrium Water.mat"));AssetDatabase.CreateAsset(wm,wp);}
            water.GetComponent<Renderer>().sharedMaterial=wm;CalmWaterSetup.Configure(water);
            var zone=water.AddComponent<WaterZone>();zone.min=new Vector2(36,65);zone.max=new Vector2(64,87);zone.surface=-.12f;
            foreach(float z in new[]{66f,78f,90f})foreach(float x in new[]{32.7f,67.3f})Box("Milk Hall Pier",new Vector3(x,6,z),new Vector3(.9f,12,.9f),white);
            foreach(float z in new[]{66f,78f,90f})Box("Milk Hall Beam",new Vector3(50,11.6f,z),new Vector3(40,.5f,.9f),white);
            Fill("Passage Soft Bounce",new Vector3(40.5f,2.1f,51.5f),1.2f,8,new Color(1,.95f,.85f));
            Fill("Small Window Bounce",new Vector3(42.4f,2.5f,42),2,6,new Color(1,.94f,.80f));
            Fill("Milk Hall Soft Bounce",new Vector3(49,8,71),24,29,new Color(1,.98f,.95f));
            Fill("Milk Hall North Bounce",new Vector3(57,8,86),18,26,new Color(.96f,.98f,1));
            Grade();
            var player=UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if(player){player.transform.SetPositionAndRotation(new Vector3(40.5f,.06f,42),Quaternion.identity);player.view.transform.localRotation=Quaternion.identity;}
            Physics.SyncTransforms();
        }
        private static void EastWindow(string name,float x,float z0,float z1,float a,float b,float sill,float head,float height,Material mat)
        {
            Box(name+" South",new Vector3(x,height/2,(z0+a)/2),new Vector3(.4f,height,a-z0),mat);
            Box(name+" North",new Vector3(x,height/2,(b+z1)/2),new Vector3(.4f,height,z1-b),mat);
            Box(name+" Below",new Vector3(x,sill/2,(a+b)/2),new Vector3(.4f,sill,b-a),mat);
            Box(name+" Above",new Vector3(x,(head+height)/2,(a+b)/2),new Vector3(.4f,height-head,b-a),mat);
            var boundary=new GameObject(name+" Boundary");boundary.transform.SetParent(root);boundary.transform.position=new Vector3(x,(sill+head)/2,(a+b)/2);boundary.AddComponent<BoxCollider>().size=new Vector3(.1f,head-sill,b-a);
        }
        private static void Ramp(Material mat)
        {
            var mesh=new Mesh{name="Milk Pool Entry Ramp"};mesh.vertices=new[]{new Vector3(47,0,65),new Vector3(47,-.695f,68),new Vector3(53,-.695f,68),new Vector3(53,0,65)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);
            const string path="Assets/Poolcore/Meshes/Milk Pool Entry Ramp.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
            var collisionSettings=new SerializedObject(mesh);collisionSettings.FindProperty("m_PreBakeTriangleCollisionMesh").boolValue=true;collisionSettings.ApplyModifiedPropertiesWithoutUndo();
            var go=new GameObject("Milk Pool Entry Ramp");go.transform.SetParent(root);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;
            GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic);
        }
        private static void Grade()
        {
            const string path="Assets/Poolcore/Settings/Milk Hall Volume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            profile.components.RemoveAll(c=>c==null);
            if(!profile.TryGet<ColorAdjustments>(out var color))color=profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(.75f);color.contrast.Override(7);color.saturation.Override(-10);
            if(!profile.TryGet<WhiteBalance>(out var balance))balance=profile.Add<WhiteBalance>(true);balance.temperature.Override(3);balance.tint.Override(7);
            foreach(var component in profile.components){if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);EditorUtility.SetDirty(component);}
            EditorUtility.SetDirty(profile);var go=new GameObject("Milk Hall Grade");go.transform.SetParent(root);go.transform.position=new Vector3(50,5,76);
            var box=go.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(40,20,36);var volume=go.AddComponent<Volume>();volume.isGlobal=false;volume.priority=7;volume.blendDistance=5;volume.sharedProfile=profile;
        }
        private static void Fill(string name,Vector3 p,float intensity,float range,Color color)
        {var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=p;var l=go.AddComponent<Light>();l.type=LightType.Point;l.intensity=intensity;l.range=range;l.color=color;l.bounceIntensity=1.2f;l.shadows=LightShadows.None;}
        private static Material Surface(string name,Color color,float grout,float smooth)
        {
            string path="Assets/Poolcore/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Poolcore/Porcelain"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetColor("_Grout",color*.75f);m.SetFloat("_GroutStrength",grout);m.SetFloat("_TileSize",.55f);m.SetFloat("_GroutWidth",.003f);m.SetFloat("_Smoothness",smooth);m.SetFloat("_TileVariation",.008f);EditorUtility.SetDirty(m);return m;
        }
        private static void Floor(string name,float x0,float x1,float z0,float z1)=>Box(name,new Vector3((x0+x1)/2,-.2f,(z0+z1)/2),new Vector3(x1-x0,.4f,z1-z0),floor);
        private static void Box(string name,Vector3 p,Vector3 size,Material mat)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.position=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);}
    }
}


