using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Poolcore.Editor
{
    public static class PoolroomsBuilder
    {
        public const string ScenePath="Assets/Poolcore/Scenes/Poolrooms.unity";
        private static Material chalk, floor, turquoise, brass, lamp, warm, dark;
        private static Transform root;
        private static Material Mat(string name, Color color, float tile=0.32f, float glaze=0.5f, float caustics=0)
        {
            string path="Assets/Poolcore/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null) { m=new Material(Shader.Find("Poolcore/Porcelain")); AssetDatabase.CreateAsset(m,path); }
            m.SetColor("_BaseColor",color); m.SetColor("_Grout",color*0.53f);
            m.SetFloat("_TileSize",tile); m.SetFloat("_Smoothness",glaze); m.SetFloat("_Caustics",caustics);
            return m;
        }
        private static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name=name;
            g.transform.SetParent(root); g.transform.position=pos; g.transform.localScale=size;
            g.GetComponent<Renderer>().sharedMaterial=mat;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
            return g;
        }
        private static void Floor(string name,float x0,float x1,float z0,float z1,float y,Material m)
        { Box(name,new Vector3((x0+x1)/2,y-0.2f,(z0+z1)/2),new Vector3(x1-x0,0.4f,z1-z0),m); }
        private static void Wall(string name,bool alongX,float fixedAt,float from,float to,float height,Material m,params float[] doors)
        {
            float cursor=from;
            for(int i=0;i<=doors.Length;i+=2)
            {
                float end=i<doors.Length?doors[i]:to;
                if(end>cursor) Box(name,alongX?new Vector3((cursor+end)/2,height/2,fixedAt):new Vector3(fixedAt,height/2,(cursor+end)/2),alongX?new Vector3(end-cursor,height,0.35f):new Vector3(0.35f,height,end-cursor),m);
                if(i<doors.Length)
                {
                    float a=doors[i],b=doors[i+1];
                    Box(name+" Lintel",alongX?new Vector3((a+b)/2,(height+3.2f)/2,fixedAt):new Vector3(fixedAt,(height+3.2f)/2,(a+b)/2),alongX?new Vector3(b-a,height-3.2f,0.35f):new Vector3(0.35f,height-3.2f,b-a),m);
                    cursor=b;
                }
            }
        }
        private static void Pool(string name,float x0,float x1,float z0,float z1,float bottom,float water,Material mat,bool steps=true)
        {
            Floor(name+" Floor",x0,x1,z0,z1,bottom,mat);
            Box(name+" West",new Vector3(x0,-0.45f,(z0+z1)/2),new Vector3(0.16f,0.9f,z1-z0),mat);
            Box(name+" East",new Vector3(x1,-0.45f,(z0+z1)/2),new Vector3(0.16f,0.9f,z1-z0),mat);
            Box(name+" North",new Vector3((x0+x1)/2,-0.45f,z1),new Vector3(x1-x0,0.9f,0.16f),mat);
            Box(name+" South",new Vector3((x0+x1)/2,-0.45f,z0),new Vector3(x1-x0,0.9f,0.16f),mat);
            if(steps)
            {
                for(int i=0;i<5;i++)
                {
                    float top=-i*0.18f;
                    Box(name+" Step "+i,new Vector3((x0+x1)/2,(top+bottom)/2,z0+0.25f+i*0.5f),new Vector3(3,top-bottom,0.5f),mat);
                }
            }
            // Water is a single plane: no collider. Wading is evaluated against its world bounds.
            var w=GameObject.CreatePrimitive(PrimitiveType.Quad); w.name=name+" Water";
            w.transform.SetParent(root); w.transform.position=new Vector3((x0+x1)/2,water,(z0+z1)/2);
            w.transform.rotation=Quaternion.Euler(90,0,0); w.transform.localScale=new Vector3(x1-x0,z1-z0,1);
            UnityEngine.Object.DestroyImmediate(w.GetComponent<Collider>());
            string path="Assets/Poolcore/Materials/"+name+" Water.mat";
            var wm=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(wm==null) { wm=new Material(Shader.Find("Poolcore/Still Water")); AssetDatabase.CreateAsset(wm,path); }
            wm.SetColor("_Shallow",new Color(0.24f,0.56f,0.53f)); wm.SetColor("_Deep",new Color(0.025f,0.22f,0.24f));
            w.GetComponent<Renderer>().sharedMaterial=wm;
            var zone=w.AddComponent<WaterZone>(); zone.min=new Vector2(x0,z0); zone.max=new Vector2(x1,z1); zone.surface=water;
        }
        private static void Glow(string name,Vector3 pos,Vector3 scale,Color color,float intensity=1.5f,float range=9)
        {
            Box(name+" Diffuser",pos,scale,lamp);
            var l=new GameObject(name+" Light").AddComponent<Light>(); l.transform.SetParent(root);
            l.transform.position=pos+Vector3.down*0.18f; l.type=LightType.Point; l.color=color; l.intensity=intensity; l.range=range;
            l.shadows=LightShadows.None;
        }
        [MenuItem("Poolcore/Create Poolrooms Experience")]
        public static void Create()
        {
            if(EditorApplication.isPlaying) throw new Exception("Stop Play Mode before generating the scene.");
            foreach(var d in new[]{"Scenes","Materials","Settings","Textures","Audio","Prefabs","UI"}) Directory.CreateDirectory("Assets/Poolcore/"+d);
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            root=new GameObject("Poolrooms Architecture").transform;
            chalk=Mat("Ivory Porcelain",new Color(0.76f,0.79f,0.72f));
            floor=Mat("Wet Ivory",new Color(0.65f,0.73f,0.69f),0.4f,0.72f);
            turquoise=Mat("Sea Glass Mosaic",new Color(0.27f,0.61f,0.59f),0.16f,0.65f,1);
            warm=Mat("Sand Porcelain",new Color(0.82f,0.71f,0.52f),0.25f,0.55f);
            dark=Mat("Deep Green Trim",new Color(0.12f,0.28f,0.26f),0.16f,0.6f);
            brass=Mat("Brass Accent",new Color(0.48f,0.38f,0.20f),2,0.65f);
            lamp=AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/Light Diffuser.mat");
            if(lamp==null) { lamp=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(lamp,"Assets/Poolcore/Materials/Light Diffuser.mat"); }
            lamp.color=Color.white; lamp.EnableKeyword("_EMISSION"); lamp.SetColor("_EmissionColor",new Color(1,0.9f,0.68f)*3);

            // 01 / Atrium. Large central pool framed by columns and slotted roof light.
            Floor("Atrium South",-12,12,-12,-4,0,floor); Floor("Atrium North",-12,12,10,14,0,floor);
            Floor("Atrium West",-12,-7,-4,10,0,floor); Floor("Atrium East",7,12,-4,10,0,floor);
            Pool("Atrium",-7,7,-4,10,-0.9f,-0.28f,turquoise);
            Wall("Atrium West",false,-12,-12,14,7.5f,chalk);
            Wall("Atrium East",false,12,-12,14,7.5f,chalk,4,7);
            Wall("Atrium South",true,-12,-12,12,7.5f,chalk);
            Wall("Atrium North",true,14,-12,12,7.5f,chalk,-6,-3);
            Floor("Roof West",-12,-2.6f,-12,14,7.7f,chalk); Floor("Roof East",2.6f,12,-12,14,7.7f,chalk);
            foreach(float z in new[]{-12f,-5f,2f,9f,14f}) Box("Roof Crossbeam",new Vector3(0,7.2f,z),new Vector3(24,0.65f,0.45f),chalk);
            for(int i=0;i<4;i++)
            {
                float z=-6+i*5;
                foreach(float x in new[]{-9.4f,9.4f})
                {
                    Box("Atrium Pillar",new Vector3(x,3.6f,z),new Vector3(0.75f,7.2f,0.75f),chalk);
                    Box("Pillar Foot",new Vector3(x,0.09f,z),new Vector3(1.1f,0.18f,1.1f),dark);
                }
            }
            // Thin waterline bands, drains and a simple ceramic bench establish scale.
            Box("West Drain",new Vector3(-7.25f,0.012f,3),new Vector3(0.08f,0.02f,14),dark);
            Box("East Drain",new Vector3(7.25f,0.012f,3),new Vector3(0.08f,0.02f,14),dark);
            Box("Bench",new Vector3(-10.9f,0.48f,-9),new Vector3(1,0.22f,4),chalk);
            foreach(float z in new[]{-10.4f,-7.6f}) Box("Bench Plinth",new Vector3(-10.9f,0.18f,z),new Vector3(0.65f,0.36f,0.4f),chalk);
            Glow("Atrium Bounce",new Vector3(0,6.8f,1),new Vector3(0.01f,0.01f,0.01f),new Color(0.78f,0.87f,1),26,24);

            // 02 / Low passage to a warmer, more intimate pool.
            Floor("East Passage",12,18,4,7,0,floor);
            Wall("Passage South",true,4,12,18,3.5f,chalk); Wall("Passage North",true,7,12,18,3.5f,chalk);
            Floor("Passage Ceiling",12,18,4,7,3.7f,chalk);
            Glow("Passage Lamp",new Vector3(15,3.35f,5.5f),new Vector3(2,0.06f,0.10f),new Color(1,0.78f,0.45f),1.5f,7);
            Floor("Warm South",18,32,1,5,0,warm); Floor("Warm North",18,32,12,16,0,warm);
            Floor("Warm West",18,21,5,12,0,warm); Floor("Warm East",29,32,5,12,0,warm);
            Pool("Warm",21,29,5,12,-0.9f,-0.27f,turquoise);
            Wall("Warm West",false,18,1,16,4.8f,warm,4,7); Wall("Warm East",false,32,1,16,4.8f,warm);
            Wall("Warm South",true,1,18,32,4.8f,warm); Wall("Warm North",true,16,18,32,4.8f,warm,24,27);
            Floor("Warm Ceiling W",18,24,1,16,5,warm); Floor("Warm Ceiling E",26,32,1,16,5,warm);
            for(int i=0;i<4;i++) Glow("Warm Cove "+i,new Vector3(31.6f,3.8f,3+i*3.5f),new Vector3(0.12f,0.1f,1.7f),new Color(1,0.73f,0.36f),2,8);
            Box("Warm Bench",new Vector3(30.9f,0.45f,8.5f),new Vector3(0.8f,0.25f,7),warm);

            // 03 / Quiet column chamber and a looping upper gallery.
            Floor("North Passage",-6,-3,14,18,0,floor);
            Wall("North Passage W",false,-6,14,18,3.5f,chalk); Wall("North Passage E",false,-3,14,18,3.5f,chalk);
            Floor("North Passage Roof",-6,-3,14,18,3.7f,chalk);
            Glow("North Passage Glow",new Vector3(-4.5f,3.35f,16),new Vector3(0.08f,0.05f,1.3f),new Color(0.65f,0.86f,1),1,6);
            Floor("Quiet South",-10,3,18,24,0,floor); Floor("Quiet North",-10,3,30,33,0,floor);
            Floor("Quiet West",-10,-8,24,30,0,floor); Floor("Quiet East",1,3,24,30,0,floor);
            Pool("Quiet",-8,1,24,30,-0.9f,-0.28f,turquoise);
            Wall("Quiet West",false,-10,18,33,6.5f,chalk); Wall("Quiet East",false,3,18,33,6.5f,chalk,20,23);
            Wall("Quiet South",true,18,-10,3,6.5f,chalk,-6,-3); Wall("Quiet North",true,33,-10,3,6.5f,chalk);
            Floor("Quiet Roof W",-10,-5,18,33,6.7f,chalk); Floor("Quiet Roof E",-3,3,18,33,6.7f,chalk);
            foreach(float x in new[]{-7.7f,0.4f}) foreach(float z in new[]{20.3f,23f,27f,31f})
                Box("Quiet Pillar",new Vector3(x,3.2f,z),new Vector3(0.55f,6.4f,0.55f),chalk);
            Glow("Quiet Bounce",new Vector3(-4,5.9f,26),new Vector3(0.01f,0.01f,0.01f),new Color(0.7f,0.85f,0.92f),14,18);
            Floor("Upper Gallery",3,27,20,23,0,floor);
            Wall("Gallery North",true,23,3,27,3.5f,chalk); Wall("Gallery South",true,20,3,27,3.5f,chalk,24,27);
            Wall("Gallery End",false,27,20,23,3.5f,chalk); Floor("Gallery Roof",3,27,20,23,3.7f,chalk);
            Floor("Warm Connector",24,27,16,20,0,warm);
            Wall("Connector W",false,24,16,20,3.5f,chalk); Wall("Connector E",false,27,16,20,3.5f,chalk);
            Floor("Connector Roof",24,27,16,20,3.7f,chalk);
            foreach(float x in new[]{6f,12f,18f,24f}) Glow("Gallery Strip",new Vector3(x,3.35f,21.5f),new Vector3(1.8f,0.06f,0.07f),new Color(0.75f,0.85f,0.84f),1.4f,7);

            var sun=new GameObject("Skylight Sun").AddComponent<Light>(); sun.type=LightType.Directional; sun.color=new Color(1,0.94f,0.80f);
            sun.intensity=2.2f; sun.shadows=LightShadows.Soft; sun.shadowBias=0.025f; sun.shadowNormalBias=0.25f; sun.transform.rotation=Quaternion.Euler(62,-28,0);
            RenderSettings.ambientMode=AmbientMode.Trilight; RenderSettings.ambientSkyColor=new Color(0.29f,0.39f,0.43f); RenderSettings.ambientEquatorColor=new Color(0.19f,0.27f,0.27f); RenderSettings.ambientGroundColor=new Color(0.12f,0.17f,0.16f);
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Exponential; RenderSettings.fogDensity=0.008f; RenderSettings.fogColor=new Color(0.36f,0.48f,0.48f);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Poolcore/Prefabs/Player.prefab");
            var player=(GameObject)PrefabUtility.InstantiatePrefab(prefab); player.transform.position=new Vector3(0,0.06f,-10);
            var cam=player.GetComponentInChildren<Camera>(); cam.backgroundColor=new Color(0.62f,0.77f,0.79f); cam.allowHDR=true;
            var data=cam.GetUniversalAdditionalCameraData(); data.renderPostProcessing=true; data.requiresColorTexture=true; data.requiresDepthTexture=true;
            // TAA, not SMAA: SMAA is a purely spatial filter, so it cannot remove the pool-edge
            // shimmer seen while walking or turning. Measured on the atrium rim at a realistic
            // per-frame view motion, TAA cuts flipping pixels ~7x versus SMAA (rot 481 -> 70 at
            // |delta|>32, 1803 -> 269 at |delta|>16). See deepseek.md.
            data.antialiasing=AntialiasingMode.TemporalAntiAliasing; data.antialiasingQuality=AntialiasingQuality.High;
            var urp=UniversalRenderPipeline.asset; if(urp==null) throw new Exception("URP must be active");
            urp.supportsHDR=true; urp.supportsCameraOpaqueTexture=true; urp.supportsCameraDepthTexture=true; urp.shadowDistance=65;
            var vp=AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Poolcore/Settings/Poolrooms Volume.asset");
            if(vp==null) { vp=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(vp,"Assets/Poolcore/Settings/Poolrooms Volume.asset"); }
            if(!vp.TryGet<Tonemapping>(out var tonemap)) tonemap=vp.Add<Tonemapping>(true); tonemap.mode.Override(TonemappingMode.ACES);
            if(!vp.TryGet<ColorAdjustments>(out var color)) color=vp.Add<ColorAdjustments>(true); color.postExposure.Override(0.55f); color.saturation.Override(-8); color.contrast.Override(8);
            if(!vp.TryGet<Bloom>(out var bloom)) bloom=vp.Add<Bloom>(true); bloom.intensity.Override(0.14f); bloom.threshold.Override(1.3f); bloom.scatter.Override(0.55f);
            if(!vp.TryGet<Vignette>(out var vignette)) vignette=vp.Add<Vignette>(true); vignette.intensity.Override(0.12f); vignette.smoothness.Override(0.7f);
            foreach(var component in vp.components) if(!AssetDatabase.Contains(component)) AssetDatabase.AddObjectToAsset(component,vp);
            var volume=new GameObject("Atmosphere").AddComponent<Volume>(); volume.isGlobal=true; volume.sharedProfile=vp;
            // Keep runtime functionality in explicit components, ready for integration tests.
            player.AddComponent<PoolAudio>();
            var experience=new GameObject("Experience").AddComponent<ExperienceMenu>(); experience.player=player.GetComponent<FirstPersonController>();
            EditorUtility.SetDirty(urp); EditorUtility.SetDirty(vp);
            EditorSceneManager.SaveScene(scene,ScenePath); EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Poolcore/Capture Pool Reflections")]
        public static void Reflections()
        {
            var waters=UnityEngine.Object.FindObjectsByType<WaterZone>();
            foreach(var w in waters) w.GetComponent<Renderer>().enabled=false;
            try
            {
                foreach(var w in waters)
                {
                    string path="Assets/Poolcore/Textures/"+w.name+" Reflection.cubemap";
                    var cube=AssetDatabase.LoadAssetAtPath<Cubemap>(path);
                    if(cube==null) { cube=new Cubemap(256,TextureFormat.RGBAHalf,true); AssetDatabase.CreateAsset(cube,path); }
                    var go=new GameObject("Reflection Capture"); var cam=go.AddComponent<Camera>();
                    cam.transform.position=w.transform.position+Vector3.up*1.2f; cam.nearClipPlane=0.1f; cam.farClipPlane=100;
                    cam.backgroundColor=new Color(0.55f,0.68f,0.69f); cam.clearFlags=CameraClearFlags.SolidColor;
                    cam.RenderToCubemap(cube); UnityEngine.Object.DestroyImmediate(go);
                    w.GetComponent<Renderer>().sharedMaterial.SetTexture("_Reflection",cube); EditorUtility.SetDirty(cube);
                }
                AssetDatabase.SaveAssets();
            }
            finally { foreach(var w in waters) w.GetComponent<Renderer>().enabled=true; }
        }
        [MenuItem("Poolcore/Bake Soft Lighting")]
        public static void Bake()
        {
            var settings=AssetDatabase.LoadAssetAtPath<LightingSettings>("Assets/Poolcore/Settings/Lighting.asset");
            if(settings==null) { settings=new LightingSettings(); AssetDatabase.CreateAsset(settings,"Assets/Poolcore/Settings/Lighting.asset"); }
            settings.bakedGI=true; settings.realtimeGI=false; settings.mixedBakeMode=MixedLightingMode.IndirectOnly;
            settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;
            settings.lightmapResolution=8; settings.lightmapMaxSize=1024;
            settings.directSampleCount=32; settings.indirectSampleCount=128; settings.environmentSampleCount=128;
            settings.maxBounces=3;
            Lightmapping.lightingSettings=settings;
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>()) l.lightmapBakeType=LightmapBakeType.Mixed;
            RenderSettings.ambientIntensity=1.3f;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            if(!Lightmapping.BakeAsync()) throw new Exception("Lighting bake could not start");
        }
        [MenuItem("Poolcore/Refine Indirect Lighting")]
        public static void RefineLighting()
        {
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>())
            {
                light.bounceIntensity=2;
                if(light.name=="Atrium Bounce Light") { light.intensity=26; light.range=24; light.color=new Color(0.78f,0.87f,1); }
                if(light.name=="Quiet Bounce Light") { light.intensity=14; light.range=18; light.color=new Color(0.7f,0.85f,0.92f); }
            }
            Bake();
        }
    }
}
