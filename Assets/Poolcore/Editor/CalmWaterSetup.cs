using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Poolcore.Editor
{
    public static class CalmWaterSetup
    {
        public static void Configure(GameObject water)
        {
            water.layer=4;
            if(!water.GetComponent<PlanarWaterReflection>()) water.AddComponent<PlanarWaterReflection>();
            var material=water.GetComponent<Renderer>().sharedMaterial;
            material.SetColor("_Deep",new Color(.045f,.24f,.23f));
            material.SetVector("_Absorption",new Vector4(.55f,.14f,.10f,0));
            material.SetFloat("_ReflectionFloor",.065f);
            material.SetFloat("_ReflectionBlur",.65f);
            if(water.name=="Depth Hall Water") {
                material.SetColor("_Deep",new Color(.025f,.15f,.20f));
                material.SetVector("_Absorption",new Vector4(.60f,.19f,.08f,0));
            }
            EditorUtility.SetDirty(material);
        }
        [MenuItem("Poolcore/Apply Calm Reflective Water")]
        public static void Apply()
        {
            if(Application.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before saving water settings.");
            foreach(var water in Object.FindObjectsByType<WaterZone>()) Configure(water.gameObject);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
