using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Poolcore
{
    public sealed class ExperienceMenu : MonoBehaviour
    {
        public FirstPersonController player;
        private GameObject panel;
        private TextMeshProUGUI title, footer, performance;
        private TMP_FontAsset font;
        private bool started, stats;
        private float nextStats, smoothDelta;
        private UniversalRenderPipelineAsset originalPipeline, runtimePipeline;
        private int quality;
        private bool automated;
        private string location;
        private float captionUntil;
        private readonly Color ink=new Color(0.88f,0.92f,0.85f);
        public bool MenuVisible => panel && panel.activeSelf;
        public void BeginAutomatedTest()
        {
            automated=true;
            player.Release();
            if(panel) panel.SetActive(false);
            if(footer) footer.text="";
            if(performance) performance.text="";
        }

        private void Start()
        {
            player.menuControlsCapture=true;
            font=TMP_FontAsset.CreateFontAsset("Microsoft YaHei","Regular",48);
            if(!font) font=TMP_FontAsset.CreateFontAsset("SimHei","Regular",48);
            if(!font) font=TMP_Settings.defaultFontAsset;
            originalPipeline=QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if(originalPipeline) { runtimePipeline=Instantiate(originalPipeline); QualitySettings.renderPipeline=runtimePipeline; }
            player.MouseSensitivity=Mathf.Clamp(PlayerPrefs.GetFloat("pool.sensitivity",player.settings.sensitivity),0.025f,0.2f);
            player.FieldOfView=Mathf.Clamp(PlayerPrefs.GetFloat("pool.fov",75),60,90);
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat("pool.volume",0.65f));
            quality=Mathf.Clamp(PlayerPrefs.GetInt("pool.quality",2),0,2); ApplyQuality(quality);
            BuildUI();
        }
        private RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=anchor; rect.pivot=new Vector2(0,1); rect.anchoredPosition=position; rect.sizeDelta=size; return rect;
        }
        private TextMeshProUGUI Label(Transform parent,string text,float x,float y,float width,float height,float size,Color? color=null)
        {
            var r=Rect("Label",parent,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,height));
            var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=text; t.fontSize=size; t.color=color??ink;
            t.raycastTarget=false; t.enableAutoSizing=false; t.textWrappingMode=TextWrappingModes.Normal;
            return t;
        }
        private UnityEngine.UI.Button Button(Transform parent,string text,float x,float y,float width,Action action)
        {
            var r=Rect(text,parent,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,52));
            var im=r.gameObject.AddComponent<UnityEngine.UI.Image>(); im.color=new Color(0.25f,0.40f,0.39f,0.85f);
            var b=r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic=im;
            var colors=b.colors; colors.highlightedColor=new Color(1.2f,1.25f,1.2f); colors.pressedColor=new Color(0.75f,0.85f,0.8f); b.colors=colors;
            b.onClick.AddListener(()=>action());
            var t=Label(r,text,20,10,width-40,34,20); t.alignment=TextAlignmentOptions.MidlineLeft;
            return b;
        }
        private void Slider(Transform parent,string text,float y,float min,float max,float value,Action<float> change,Func<float,string> format)
        {
            Label(parent,text,36,y,200,30,18);
            var number=Label(parent,format(value),292,y,140,30,17,new Color(0.64f,0.76f,0.71f)); number.alignment=TextAlignmentOptions.TopRight;
            var r=Rect(text,parent,new Vector2(0,1),new Vector2(36,-y-37),new Vector2(396,22));
            var hit=r.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color=new Color(0,0,0,0);
            var slider=r.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.minValue=min; slider.maxValue=max;
            var track=Rect("Track",r,new Vector2(0,1),new Vector2(0,-9),new Vector2(396,3));
            track.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0.26f,0.37f,0.35f);
            var area=Rect("Handle Area",r,new Vector2(0,1),Vector2.zero,new Vector2(396,22));
            var handle=Rect("Handle",area,new Vector2(0,0.5f),Vector2.zero,new Vector2(12,22)); handle.pivot=new Vector2(0.5f,0.5f);
            var handleImage=handle.gameObject.AddComponent<UnityEngine.UI.Image>(); handleImage.color=ink;
            slider.handleRect=handle; slider.targetGraphic=handleImage; slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v=>{ number.text=format(v); change(v); });
        }
        private void BuildUI()
        {
            var canvasGO=new GameObject("Poolrooms Interface",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGO.transform.SetParent(transform,false); var canvas=canvasGO.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=0.5f;
            if(!FindAnyObjectByType<EventSystem>())
            { var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); es.transform.SetParent(transform); }
            var overlay=Rect("Pause Overlay",canvas.transform,new Vector2(0,1),Vector2.zero,Vector2.zero);
            overlay.anchorMin=Vector2.zero; overlay.anchorMax=Vector2.one; overlay.offsetMin=overlay.offsetMax=Vector2.zero;
            overlay.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0.02f,0.08f,0.085f,0.30f); panel=overlay.gameObject;
            var card=Rect("Menu Card",overlay,new Vector2(0,0.5f),new Vector2(96,370),new Vector2(468,740));
            card.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(0.035f,0.105f,0.11f,0.96f);
            Label(card,"P O O L R O O M S",36,32,410,40,27);
            Label(card,"漂浮之间",36,88,410,66,44);
            title=Label(card,"没有任务。沿着水与光，慢慢走。",36,164,400,50,18,new Color(0.64f,0.76f,0.71f));
            Button(card,"进入空间  →",36,225,396,Resume);
            Slider(card,"视角灵敏度",302,0.025f,0.2f,player.MouseSensitivity,v=>{player.MouseSensitivity=v; PlayerPrefs.SetFloat("pool.sensitivity",v);},v=>v.ToString("0.000"));
            Slider(card,"视野范围",385,60,90,player.FieldOfView,v=>{player.FieldOfView=v; PlayerPrefs.SetFloat("pool.fov",v);},v=>Mathf.RoundToInt(v)+"°");
            Slider(card,"环境音量",468,0,1,AudioListener.volume,v=>{AudioListener.volume=v; PlayerPrefs.SetFloat("pool.volume",v);},v=>Mathf.RoundToInt(v*100)+"%");
            UnityEngine.UI.Button q=null;
            q=Button(card,"画质 · "+QualityName(),36,552,190,()=>{quality=(quality+1)%3; ApplyQuality(quality); q.GetComponentInChildren<TextMeshProUGUI>().text="画质 · "+QualityName(); PlayerPrefs.SetInt("pool.quality",quality);});
            Button(card,"回到入口",242,552,190,()=>{player.ResetToSpawn(); Resume();});
            Button(card,"退出",36,620,396,Quit);
            Label(card,"WASD 移动  ·  鼠标环顾  ·  Shift 快走\nEsc 设置  ·  F2 拍照  ·  F3 性能",36,682,420,55,15,new Color(0.58f,0.70f,0.66f));
            var f=Rect("Location",canvas.transform,new Vector2(0,0),new Vector2(45,60),new Vector2(760,35));
            footer=f.gameObject.AddComponent<TextMeshProUGUI>(); footer.font=font; footer.fontSize=16; footer.color=ink; footer.raycastTarget=false;
            var perf=Rect("Performance",canvas.transform,new Vector2(1,1),new Vector2(-240,-30),new Vector2(210,60));
            performance=perf.gameObject.AddComponent<TextMeshProUGUI>(); performance.font=font; performance.fontSize=16; performance.color=ink; performance.raycastTarget=false;
        }
        private string QualityName() => new[]{"轻量","均衡","精细"}[quality];
        public void ApplyQuality(int value)
        {
            quality=Mathf.Clamp(value,0,2);
            // MSAA must stay off: URP silently disables TAA while MSAA is on ("Disabling TAA
            // because MSAA is on"), and TAA is what removes the pool-edge shimmer seen while
            // moving the view. MSAA also cannot help the shader-level aliasing that causes it.
            if(runtimePipeline) { runtimePipeline.renderScale=new[]{0.75f,0.9f,1f}[quality]; runtimePipeline.shadowDistance=new[]{25f,45f,65f}[quality]; runtimePipeline.msaaSampleCount=1; }
            QualitySettings.vSyncCount=1; Application.targetFrameRate=120;
        }
        public void Resume() { started=true; PlayerPrefs.Save(); player.Capture(); if(panel) panel.SetActive(false); }
        private void Quit()
        {
            PlayerPrefs.Save();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
            #else
            Application.Quit();
            #endif
        }
        private void Update()
        {
            if(!panel || automated) return;
            bool show=!player.IsCaptured;
            if(panel.activeSelf!=show) { panel.SetActive(show); if(show) title.text=started?"停一会儿，再继续探索。":"没有任务。沿着水与光，慢慢走。"; }
            if(Keyboard.current!=null && Keyboard.current.f3Key.wasPressedThisFrame) stats=!stats;
            smoothDelta=Mathf.Lerp(smoothDelta,Time.unscaledDeltaTime,0.05f);
            if(Time.unscaledTime>nextStats) { nextStats=Time.unscaledTime+0.5f; performance.text=stats?$"{1/Mathf.Max(0.001f,smoothDelta):F0} FPS  /  {smoothDelta*1000:F1} ms":""; }
            string nextLocation=player.transform.position.z>18 ? "03  /  静水回廊" : player.transform.position.x>18 ? "02  /  暖光池" : "01  /  天光泳池";
            if(!show && nextLocation!=location) { location=nextLocation; footer.text=location; captionUntil=Time.unscaledTime+4; }
            if(show || Time.unscaledTime>captionUntil) footer.text="";
            if(!show && Keyboard.current!=null && Keyboard.current.f2Key.wasPressedThisFrame)
                StartCoroutine(TakePhoto());
        }
        private System.Collections.IEnumerator TakePhoto()
        {
            string dir=Path.Combine(Application.persistentDataPath,"Photos"); Directory.CreateDirectory(dir);
            performance.gameObject.SetActive(false); footer.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".png"));
            yield return null;
            performance.gameObject.SetActive(true); footer.gameObject.SetActive(true);
            footer.text="照片已保存 · "+dir; captionUntil=Time.unscaledTime+5;
        }
        private void OnDestroy()
        {
            PlayerPrefs.Save();
            if(runtimePipeline) { QualitySettings.renderPipeline=originalPipeline; Destroy(runtimePipeline); }
            if(font && font!=TMP_Settings.defaultFontAsset) { foreach(var atlas in font.atlasTextures) if(atlas) Destroy(atlas); if(font.material) Destroy(font.material); Destroy(font); }
        }
    }
}
