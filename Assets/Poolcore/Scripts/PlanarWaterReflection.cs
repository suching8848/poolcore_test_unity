using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Poolcore
{
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public sealed class PlanarWaterReflection : MonoBehaviour
    {
        private Camera reflectionCamera;
        private RenderTexture texture;
        private Renderer surface;
        private MaterialPropertyBlock properties;
        private readonly Plane[] frustum=new Plane[6];
        private readonly UniversalRenderPipeline.SingleCameraRequest request=new UniversalRenderPipeline.SingleCameraRequest();
        private static bool rendering;
        public int RenderCount { get; private set; }
        public RenderTexture ReflectionTexture => texture;
        private void OnEnable()
        {
            surface=GetComponent<Renderer>(); properties=new MaterialPropertyBlock();
            RenderPipelineManager.beginCameraRendering+=RenderReflection;
        }
        private void RenderReflection(ScriptableRenderContext context,Camera source)
        {
            if(rendering || !surface || !surface.enabled || source.cameraType==CameraType.Reflection || source.cameraType==CameraType.Preview) return;
            if(source.cameraType!=CameraType.Game && source.cameraType!=CameraType.SceneView) return;
            if(source.transform.position.y<=transform.position.y+.03f) return;
            GeometryUtility.CalculateFrustumPlanes(source,frustum);
            if(!GeometryUtility.TestPlanesAABB(frustum,surface.bounds)) return;
            if(Vector3.Distance(source.transform.position,surface.bounds.ClosestPoint(source.transform.position))>65) return;
            int width=Mathf.Clamp(source.pixelWidth/2,256,1024);
            int height=Mathf.Max(128,Mathf.RoundToInt(width/(float)Mathf.Max(1,source.pixelWidth)*source.pixelHeight));
            if(!texture || texture.width!=width || texture.height!=height) {
                ReleaseTexture();
                texture=new RenderTexture(width,height,24,RenderTextureFormat.ARGBHalf) {
                    name=name+" Planar Reflection",hideFlags=HideFlags.HideAndDontSave,
                    useMipMap=true,autoGenerateMips=false,filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp
                }; texture.Create();
            }
            if(!reflectionCamera) {
                var go=new GameObject(name+" Reflection Camera") {hideFlags=HideFlags.HideAndDontSave};
                reflectionCamera=go.AddComponent<Camera>(); reflectionCamera.enabled=false;
                go.AddComponent<UniversalAdditionalCameraData>();
            }
            reflectionCamera.CopyFrom(source); reflectionCamera.enabled=false;
            reflectionCamera.cameraType=CameraType.Reflection; reflectionCamera.useOcclusionCulling=false;
            reflectionCamera.cullingMask=source.cullingMask & ~(1<<4);
            reflectionCamera.allowMSAA=false; reflectionCamera.allowHDR=true;
            var data=reflectionCamera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing=false;data.antialiasing=AntialiasingMode.None;
            data.requiresColorOption=CameraOverrideOption.Off;data.requiresDepthOption=CameraOverrideOption.Off;
            data.renderShadows=true;
            float y=transform.position.y;
            var mirror=Matrix4x4.identity;mirror.m11=-1;mirror.m13=2*y;
            reflectionCamera.transform.SetPositionAndRotation(mirror.MultiplyPoint(source.transform.position),
                Quaternion.LookRotation(mirror.MultiplyVector(source.transform.forward),mirror.MultiplyVector(source.transform.up)));
            reflectionCamera.worldToCameraMatrix=source.worldToCameraMatrix*mirror;
            reflectionCamera.projectionMatrix=source.nonJitteredProjectionMatrix;
            var view=reflectionCamera.worldToCameraMatrix;
            // Keep CPU visibility/shadow culling on the ordinary mirrored frustum.
            // The oblique near plane is only for GPU clipping: its displaced far
            // plane can send native frustum-corner reconstruction through infinity.
            reflectionCamera.cullingMatrix=source.nonJitteredProjectionMatrix*view;
            var point=view.MultiplyPoint(new Vector3(0,y+.015f,0));
            var normal=view.MultiplyVector(Vector3.up).normalized;
            var plane=new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(point,normal));
            reflectionCamera.projectionMatrix=reflectionCamera.CalculateObliqueMatrix(plane);
            bool invert=GL.invertCulling;
            try {
                rendering=true;GL.invertCulling=!invert;
                request.destination=texture;
                RenderPipeline.SubmitRenderRequest(reflectionCamera,request);
                texture.GenerateMips();RenderCount++;
                surface.GetPropertyBlock(properties);
                properties.SetTexture("_PlanarReflection",texture);
                // SubmitRenderRequest's final blit normalizes the texture orientation.
                // Applying the GPU render-target Y flip here would invert the sampled image.
                properties.SetMatrix("_ReflectionVP",reflectionCamera.projectionMatrix*view);
                properties.SetFloat("_HasPlanarReflection",1);
                surface.SetPropertyBlock(properties);
            } finally {GL.invertCulling=invert;rendering=false;}
        }
        private void ReleaseTexture() {if(texture) {texture.Release();DestroyResource(texture);texture=null;}}
        private static void DestroyResource(Object obj) {if(Application.isPlaying) Destroy(obj);else DestroyImmediate(obj);}
        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering-=RenderReflection;
            if(surface) {surface.GetPropertyBlock(properties);properties.SetFloat("_HasPlanarReflection",0);surface.SetPropertyBlock(properties);}
            ReleaseTexture();if(reflectionCamera) DestroyResource(reflectionCamera.gameObject);
        }
    }
}



