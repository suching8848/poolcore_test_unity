using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

namespace Poolcore.Editor
{
    public static class FoundationBuilder
    {
        public const string ScenePath = "Assets/Poolcore/Scenes/Foundation.unity";
        private static Material Material(string name, Color color)
        {
            string path = "Assets/Poolcore/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            material.color = color;
            material.SetFloat("_Smoothness", 0.18f);
            return material;
        }
        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        [MenuItem("Poolcore/Create Foundation Scene")]
        public static void Create()
        {
            foreach (var folder in new[] { "Scenes", "Materials", "Prefabs", "Settings", "Audio" })
                Directory.CreateDirectory("Assets/Poolcore/" + folder);
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ivory = Material("Chalk", new Color(0.74f, 0.77f, 0.74f));
            var teal = Material("Basin", new Color(0.27f, 0.52f, 0.54f));
            var dark = Material("Edge", new Color(0.15f, 0.27f, 0.30f));
            var sand = Material("Steps", new Color(0.67f, 0.60f, 0.44f));
            // Pool opening x=-4..4,z=0..8. Dry concourse is y=0, basin floor y=-0.8.
            Box("South Concourse", new Vector3(0,-0.25f,-5), new Vector3(24,0.5f,10), ivory);
            Box("West Concourse", new Vector3(-8,-0.25f,4), new Vector3(8,0.5f,8), ivory);
            Box("East Concourse", new Vector3(8,-0.25f,4), new Vector3(8,0.5f,8), ivory);
            Box("North Concourse", new Vector3(0,-0.25f,10), new Vector3(24,0.5f,4), ivory);
            Box("Empty Shallow Basin", new Vector3(0,-1.05f,4), new Vector3(8,0.5f,8), teal);
            Box("West Wall", new Vector3(-12.2f,2.5f,1), new Vector3(0.4f,5,22.4f), ivory);
            Box("East Wall", new Vector3(12.2f,2.5f,1), new Vector3(0.4f,5,22.4f), ivory);
            Box("South Wall", new Vector3(0,2.5f,-10.2f), new Vector3(24,5,0.4f), ivory);
            Box("North Wall", new Vector3(0,2.5f,12.2f), new Vector3(24,5,0.4f), ivory);
            // Four steps descend from 0 to -0.8; box bottoms extend into basin foundation.
            for (int i=0; i<4; i++)
            {
                float top = -0.2f*i;
                Box("Basin Stair " + i, new Vector3(0,(top-0.8f)*0.5f,0.25f+i*0.5f), new Vector3(3,top+0.8f,0.5f), sand);
            }
            for (int i=0; i<3; i++)
            {
                Box("Column W"+i, new Vector3(-7,2.5f,0+i*4), new Vector3(0.8f,5,0.8f), ivory);
                Box("Column E"+i, new Vector3(7,2.5f,0+i*4), new Vector3(0.8f,5,0.8f), ivory);
            }
            var ramp = Box("Ramp 15deg", new Vector3(-8,0.4f,-5), new Vector3(2.5f,0.3f,4), sand);
            ramp.transform.rotation = Quaternion.Euler(-15,0,0);
            Box("Ramp Landing", new Vector3(-8,0.415f,-2.4f), new Vector3(2.5f,0.83f,1.4f), sand);
            Box("Corner Test A", new Vector3(8,0.8f,-5), new Vector3(0.4f,1.6f,3), dark);
            Box("Corner Test B", new Vector3(9,0.8f,-3.7f), new Vector3(2.4f,1.6f,0.4f), dark);
            var settings = AssetDatabase.LoadAssetAtPath<MovementSettings>("Assets/Poolcore/Settings/Movement.asset");
            if (settings == null) { settings = ScriptableObject.CreateInstance<MovementSettings>(); AssetDatabase.CreateAsset(settings,"Assets/Poolcore/Settings/Movement.asset"); }
            var player = new GameObject("Player"); player.transform.position = new Vector3(0,0.08f,-7);
            var body = player.AddComponent<CharacterController>(); body.height = 1.8f; body.radius = 0.3f;
            body.center = new Vector3(0,0.9f,0); body.stepOffset = 0.28f; body.slopeLimit = 45; body.skinWidth = 0.025f; body.minMoveDistance = 0;
            var cameraGo = new GameObject("First Person Camera"); cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(player.transform, false); cameraGo.transform.localPosition = new Vector3(0,1.65f,0);
            var camera = cameraGo.AddComponent<Camera>(); camera.nearClipPlane = 0.08f; camera.farClipPlane = 150;
            camera.fieldOfView = settings.fieldOfView; cameraGo.AddComponent<AudioListener>();
            var controller = player.AddComponent<FirstPersonController>(); controller.settings = settings; controller.view = camera;
            PrefabUtility.SaveAsPrefabAsset(player, "Assets/Poolcore/Prefabs/Player.prefab");
            var light = new GameObject("Daylight").AddComponent<Light>(); light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(48,-32,0); light.intensity=1.6f; light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f,0.55f,0.60f);
            RenderSettings.skybox = null; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.48f,0.61f,0.67f);
            PlayerSettings.companyName = "Poolcore"; PlayerSettings.productName = "Poolcore Foundation";
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true) };
            AssetDatabase.SaveAssets();
        }
        public static void Screenshot()
        {
            Directory.CreateDirectory("artifacts");
            var camera=Camera.main; var rt = new RenderTexture(1280,720,24);
            var old= camera.targetTexture; var active = RenderTexture.active;
            try { camera.targetTexture=rt; camera.Render(); RenderTexture.active=rt;
                var tex=new Texture2D(1280,720,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
                File.WriteAllBytes("artifacts/foundation.png",tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            } finally { camera.targetTexture=old; RenderTexture.active=active; rt.Release(); Object.DestroyImmediate(rt); }
        }
        [MenuItem("Poolcore/Build Windows Foundation")]
        public static void ScheduleBuild() { EditorApplication.delayCall += Build; }
        public static void Build()
        {
            Directory.CreateDirectory("artifacts");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath}, locationPathName="Builds/Windows/Poolcore.exe", target=BuildTarget.StandaloneWindows64, options=BuildOptions.Development });
            File.WriteAllText("artifacts/build-result.txt",report.summary.result+" | "+report.summary.totalErrors+" errors | "+report.summary.totalSize+" bytes");
            if(report.summary.result!=BuildResult.Succeeded) throw new System.Exception("Windows build failed: "+report.summary.result);
        }
    }
}
