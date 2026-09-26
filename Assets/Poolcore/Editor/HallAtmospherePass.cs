using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Poolcore.Editor
{
    public static class HallAtmospherePass
    {
        private const string RootName="Depth Hall Details";
        [MenuItem("Poolcore/Refine Depth Hall Atmosphere")]
        public static void Apply()
        {
            if(Application.isPlaying || EditorSceneManager.GetActiveScene().path!=PoolroomsBuilder.ScenePath)
                throw new InvalidOperationException("Open Poolrooms outside Play Mode.");
            Directory.CreateDirectory("artifacts/depth-hall");
            File.Copy(PoolroomsBuilder.ScenePath,"artifacts/depth-hall/Atmosphere-before-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity",true);
            Configure();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        public static void Configure()
        {
            var hall=GameObject.Find("Depth Hall Architecture");if(!hall)return;
            foreach(var light in hall.GetComponentsInChildren<Light>()) if(light.name=="Hall Soft Fill") {
                light.intensity=14;light.range=25;light.color=new Color(.88f,.94f,1);light.bounceIntensity=1.1f;
            }
            var ivory=Surface("Depth Hall Ivory",new Color(.86f,.84f,.78f),.85f,.32f,.14f);
            Surface("Depth Hall Deck",new Color(.74f,.77f,.73f),.6f,.42f,.22f);
            var ceiling=Surface("Depth Hall Ceiling",new Color(.84f,.83f,.79f),1.2f,.2f,.10f);
            foreach(var renderer in hall.GetComponentsInChildren<MeshRenderer>())
                if(renderer.name=="Hall Roof" || renderer.name=="Hall Roof Beam") renderer.sharedMaterial=ceiling;
            var found=GameObject.Find(RootName);
            if(found) UnityEngine.Object.DestroyImmediate(found);
            var root=new GameObject(RootName).transform;root.SetParent(hall.transform);
            root.gameObject.AddComponent<HallRoomTone>();
            var stone=Surface("Depth Hall Coping",new Color(.81f,.80f,.72f),.7f,.36f,.16f);
            var dark=Surface("Depth Hall Drain Bed",new Color(.09f,.13f,.13f),5,.22f,.1f);
            var metal=Surface("Depth Hall Grate",new Color(.48f,.53f,.51f),5,.65f,.05f);
            // Entirely on the deck, with a physical gap above it: no coplanar overlays.
            foreach(float x in new[]{43.78f,72.22f}) Box(root,"Hall Coping",new Vector3(x,.025f,16),new Vector3(.30f,.05f,28),stone);
            Box(root,"Hall North Coping",new Vector3(58,.025f,30.22f),new Vector3(28,.05f,.30f),stone);
            foreach(float x in new[]{43.35f,72.65f}) Box(root,"Hall Drain Channel",new Vector3(x,.006f,16),new Vector3(.24f,.012f,28),dark);
            Box(root,"Hall North Drain Channel",new Vector3(58,.006f,30.65f),new Vector3(28,.012f,.24f),dark);
            Grates(root,metal);
            foreach(float z in new[]{8f,26f}) {
                Box(root,"Hall Vent Recess",new Vector3(77.78f,2.6f,z),new Vector3(.06f,.55f,1.1f),dark);
                for(int i=0;i<6;i++)Box(root,"Hall Vent Louver",new Vector3(77.73f,2.38f+i*.085f,z),new Vector3(.04f,.025f,1.04f),metal);
            }
            foreach(float x in new[]{38.25f,77.75f})
                Box(root,"Hall Upper Frieze",new Vector3(x,8.9f,16),new Vector3(.12f,.18f,39.5f),stone);
            foreach(float z in new[]{-3.75f,35.75f})
                Box(root,"Hall Upper Frieze",new Vector3(58,8.9f,z),new Vector3(39.5f,.18f,.12f),stone);
            // Recessed roof edges give the skylights an architectural thickness.
            foreach(float x in new[]{49f,55f,65f,71f})
                Box(root,"Hall Skylight Reveal",new Vector3(x,10.82f,16),new Vector3(.18f,.40f,40),ivory);
            LocalGrade(root);
            HallArchitecturePass.Configure();
            Physics.SyncTransforms();
        }
        private static Material Surface(string name,Color color,float tiles,float smooth,float grout)
        {
            string path="Assets/Poolcore/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Poolcore/Porcelain"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetColor("_Grout",color*.65f);m.SetFloat("_TileSize",tiles);
            m.SetFloat("_Smoothness",smooth);m.SetFloat("_GroutStrength",grout);EditorUtility.SetDirty(m);return m;
        }
        private static GameObject Box(Transform root,string name,Vector3 p,Vector3 size,Material m)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);
            g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);
            // These small cosmetic details must not snag the walking controller.
            UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        private static void Grates(Transform root,Material m)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();
            Action<Vector3,Vector3,Vector3> quad=(center,right,forward)=>{
                int i=vertices.Count;vertices.Add(center-right-forward);vertices.Add(center-right+forward);
                vertices.Add(center+right+forward);vertices.Add(center+right-forward);
                indices.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});
            };
            for(float z=2.05f;z<30;z+=.12f) foreach(float x in new[]{43.35f,72.65f})
                quad(new Vector3(x,.018f,z),Vector3.right*.12f,Vector3.forward*.028f);
            for(float x=44.05f;x<72;x+=.12f) quad(new Vector3(x,.018f,30.65f),Vector3.right*.028f,Vector3.forward*.12f);
            var mesh=new Mesh{name="Hall Drain Grates"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            const string path="Assets/Poolcore/Meshes/Hall Drain Grates.asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
            var go=new GameObject("Hall Drain Grates");go.transform.SetParent(root);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=m;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        private static void LocalGrade(Transform root)
        {
            var urp=UniversalRenderPipeline.asset;var camera=Camera.main;
            if(!urp || !urp.supportsHDR || !camera || !camera.GetUniversalAdditionalCameraData().renderPostProcessing)
                throw new InvalidOperationException("HDR URP and camera post-processing must be enabled.");
            const string path="Assets/Poolcore/Settings/Depth Hall Volume.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            if(!profile.TryGet<ColorAdjustments>(out var grade))grade=profile.Add<ColorAdjustments>(true);
            grade.postExposure.Override(.45f);grade.contrast.Override(12);grade.saturation.Override(-6);
            if(!profile.TryGet<WhiteBalance>(out var balance))balance=profile.Add<WhiteBalance>(true);
            balance.temperature.Override(4);balance.tint.Override(4);
            foreach(var component in profile.components)if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);
            var go=new GameObject("Hall Atmosphere Volume");go.transform.SetParent(root);go.transform.position=new Vector3(58,4,16);
            var bounds=go.AddComponent<BoxCollider>();bounds.isTrigger=true;bounds.size=new Vector3(40,18,40);
            var volume=go.AddComponent<Volume>();volume.isGlobal=false;volume.priority=5;volume.blendDistance=5;volume.sharedProfile=profile;
            EditorUtility.SetDirty(profile);
        }
    }
}
