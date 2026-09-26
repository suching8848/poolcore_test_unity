using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Poolcore.Editor
{
    public static class HallArchitecturePass
    {
        private const string RootName="Hall Architectural Finish";
        [MenuItem("Poolcore/Finish Hall Architecture and Materials")]
        public static void Apply()
        {
            if(Application.isPlaying || EditorSceneManager.GetActiveScene().path!=PoolroomsBuilder.ScenePath)
                throw new InvalidOperationException("Open Poolrooms outside Play Mode.");
            Directory.CreateDirectory("artifacts/hall-finish");
            File.Copy(PoolroomsBuilder.ScenePath,"artifacts/hall-finish/Before-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity",true);
            Configure();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        public static void Configure()
        {
            var hall=GameObject.Find("Depth Hall Architecture");if(!hall)return;
            var previous=GameObject.Find(RootName);if(previous)UnityEngine.Object.DestroyImmediate(previous);
            var root=new GameObject(RootName).transform;root.SetParent(hall.transform);
            var wall=Get("Depth Hall Ivory");
            wall.SetColor("_BaseColor",new Color(.83f,.84f,.79f));wall.SetVector("_TileRatio",new Vector4(2,1,0,0));wall.SetFloat("_TileSize",.3f);
            wall.SetFloat("_GroutWidth",.004f);wall.SetFloat("_GroutStrength",.23f);wall.SetFloat("_UpperSurface",1);wall.SetFloat("_TileHeight",2.4f);
            wall.SetColor("_UpperColor",new Color(.84f,.83f,.78f));wall.SetFloat("_TileVariation",.012f);wall.SetFloat("_RoughnessVariation",.045f);EditorUtility.SetDirty(wall);
            var deck=Get("Depth Hall Deck");deck.SetFloat("_TileSize",.55f);deck.SetVector("_TileRatio",new Vector4(1,1,0,0));deck.SetFloat("_GroutWidth",.0035f);
            deck.SetFloat("_GroutStrength",.35f);deck.SetFloat("_Smoothness",.26f);deck.SetFloat("_TileVariation",.018f);deck.SetFloat("_RoughnessVariation",.06f);
            deck.SetFloat("_WetEdge",.7f);deck.SetFloat("_WetWidth",.7f);EditorUtility.SetDirty(deck);
            var tile=Get("Depth Hall Pool Tile");tile.SetFloat("_TileSize",.22f);tile.SetFloat("_GroutWidth",.006f);tile.SetFloat("_GroutStrength",.23f);
            tile.SetFloat("_TileVariation",.018f);tile.SetFloat("_RoughnessVariation",.035f);EditorUtility.SetDirty(tile);
            var ceiling=Get("Depth Hall Ceiling");ceiling.SetFloat("_GroutStrength",0);ceiling.SetFloat("_TileVariation",.005f);ceiling.SetFloat("_Smoothness",.14f);EditorUtility.SetDirty(ceiling);
            var stone=Get("Depth Hall Coping");stone.SetFloat("_TileSize",.7f);stone.SetFloat("_GroutWidth",.002f);stone.SetFloat("_GroutStrength",.24f);
            stone.SetFloat("_TileVariation",.009f);stone.SetFloat("_RoughnessVariation",.035f);stone.SetFloat("_WetEdge",.35f);EditorUtility.SetDirty(stone);
            var plaster=Copy("Depth Hall Structural Plaster",ceiling);plaster.SetColor("_BaseColor",new Color(.86f,.85f,.80f));EditorUtility.SetDirty(plaster);
            var renderers=hall.GetComponentsInChildren<MeshRenderer>();
            foreach(var r in renderers.Where(r=>r.name=="Hall Pillar")) {
                r.transform.localScale=new Vector3(.85f,11,.85f);r.sharedMaterial=plaster;
                var p=r.transform.position;
                Box(root,"Hall Pillar Capital",new Vector3(p.x,10.40f,p.z),new Vector3(1.20f,.24f,1.2f),plaster,false);
                Box(root,"Hall Pillar Collar",new Vector3(p.x,.50f,p.z),new Vector3(.91f,.60f,.91f),stone,false);
            }
            foreach(var r in renderers.Where(r=>r.name=="Hall Pillar Plinth")) {
                r.transform.localScale=new Vector3(1.12f,.20f,1.12f);r.sharedMaterial=stone;
            }
            var beams=renderers.Where(r=>r.name=="Hall Roof Beam").OrderBy(r=>r.transform.position.z).ToArray();
            for(int i=0;i<beams.Length;i++) {
                beams[i].transform.position=new Vector3(58,10.7f,2+8*i);beams[i].transform.localScale=new Vector3(40,.44f,.48f);beams[i].sharedMaterial=plaster;
            }
            foreach(var r in renderers.Where(r=>r.name=="Hall Roof")) if(r.transform.position.x>55 && r.transform.position.x<65) {
                r.transform.position=new Vector3(61,11.1f,16);r.transform.localScale=new Vector3(12,.3f,40);
            }
            foreach(var r in renderers.Where(r=>r.name=="Hall Skylight Reveal")) if(Mathf.Abs(r.transform.position.x-65)<.1f)
                r.transform.position=new Vector3(67,r.transform.position.y,r.transform.position.z);
            // Replace only the solid wall behind the new recess. The original
            // warm-room connector and the entire pool collision remain intact.
            var oldWall=GameObject.Find("Hall West North");if(oldWall)UnityEngine.Object.DestroyImmediate(oldWall);
            Box(root,"Hall West Recess South",new Vector3(38,5.5f,15),new Vector3(.35f,11,8),wall);
            Box(root,"Hall West Recess North",new Vector3(38,5.5f,30.5f),new Vector3(.35f,11,11),wall);
            Box(root,"Hall West Recess Header",new Vector3(38,7.255f,22),new Vector3(.35f,7.49f,6),wall);
            Box(root,"Recess Floor",new Vector3(36,-.2f,22),new Vector3(4,.4f,6),deck);
            Box(root,"Recess Back Wall",new Vector3(34,1.755f,22),new Vector3(.3f,3.51f,6),wall);
            Box(root,"Recess South Wall",new Vector3(36,1.755f,19),new Vector3(4,3.51f,.3f),wall);
            Box(root,"Recess North Wall",new Vector3(36,1.755f,25),new Vector3(4,3.51f,.3f),wall);
            // Three adjoining slabs, never coplanar overlapping roof faces.
            Box(root,"Recess Ceiling",new Vector3(37.825f,3.65f,22),new Vector3(7.65f,.28f,6),plaster);
            Box(root,"Low Gallery South Ceiling",new Vector3(39.925f,3.65f,15.5f),new Vector3(3.45f,.28f,7),plaster);
            Box(root,"Low Gallery North Ceiling",new Vector3(39.925f,3.65f,28),new Vector3(3.45f,.28f,6),plaster);
            foreach(float z in new[]{18f,26f}) {
                Box(root,"Low Gallery Column Head",new Vector3(40.5f,3.36f,z),new Vector3(1.15f,.3f,1.15f),stone,false);
            }
            Box(root,"Recess Bench",new Vector3(34.65f,.43f,22),new Vector3(.7f,.16f,4.5f),stone);
            foreach(float z in new[]{20.3f,23.7f})Box(root,"Recess Bench Leg",new Vector3(34.65f,.175f,z),new Vector3(.5f,.35f,.25f),stone);
            var lamp=Copy("Hall Recess Lamp",plaster);lamp.shader=Shader.Find("Universal Render Pipeline/Lit");
            lamp.SetColor("_BaseColor",new Color(.8f,.73f,.56f));lamp.SetColor("_EmissionColor",new Color(1,.82f,.60f));lamp.EnableKeyword("_EMISSION");
            lamp.SetFloat("_Smoothness",.3f);lamp.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;EditorUtility.SetDirty(lamp);
            Box(root,"Recess Concealed Light Housing",new Vector3(34.27f,2.96f,22),new Vector3(.20f,.10f,3.6f),stone,false);
            Box(root,"Recess Light Diffuser",new Vector3(34.39f,2.97f,22),new Vector3(.03f,.045f,3.4f),lamp,false);
            var lightObject=new GameObject("Recess Warm Fill");lightObject.transform.SetParent(root);lightObject.transform.position=new Vector3(35.2f,2.8f,22);
            var light=lightObject.AddComponent<Light>();light.type=LightType.Point;light.intensity=2.4f;light.range=5;light.color=new Color(1,.82f,.60f);
            light.bounceIntensity=1.2f;light.shadows=LightShadows.Soft;light.shadowBias=.015f;light.shadowNormalBias=.08f;
            // A five-metre alcove light does not need six high-resolution faces.
            var shadowData=lightObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            var serializedLight=new SerializedObject(shadowData);
            serializedLight.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;
            serializedLight.ApplyModifiedPropertiesWithoutUndo();
            // Thin cap follows the wainscot height without covering the doorway.
            foreach(float z in new[]{-3.76f,35.76f})Box(root,"Hall Tile Cap",new Vector3(58,2.4f,z),new Vector3(39.5f,.035f,.045f),stone,false);
            Box(root,"Hall East Tile Cap",new Vector3(77.76f,2.4f,16),new Vector3(.045f,.035f,39.5f),stone,false);
            Physics.SyncTransforms();
        }
        private static Material Get(string name)
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Poolcore/Materials/"+name+".mat");
            if(!m)throw new InvalidOperationException("Apply the hall atmosphere pass first: "+name);return m;
        }
        private static Material Copy(string name,Material source)
        {
            string path="Assets/Poolcore/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(source);AssetDatabase.CreateAsset(material,path);}return material;
        }
        private static GameObject Box(Transform parent,string name,Vector3 position,Vector3 scale,Material material,bool collision=true)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=position;g.transform.localScale=scale;
            g.GetComponent<Renderer>().sharedMaterial=material;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);
            if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
    }
}
