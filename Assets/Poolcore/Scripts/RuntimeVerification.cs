using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Poolcore
{
    // Opt-in built-player validation. Normal launches never create this component.
    public sealed class RuntimeVerification : MonoBehaviour
    {
        [Serializable] private class Report
        {
            public string unityVersion, gpu, result;
            public int width,height,frames,waypoints,errors,stallsOver100ms;
            public int drySteps,wadingSteps,wadingVariants,roomEmitters,worldLabels;
            public float seconds,medianMs,p95Ms,p99Ms,averageFps;
        }
        private readonly List<float> timings=new List<float>();
        private readonly List<string> errors=new List<string>();
        private FirstPersonController player;
        private ExperienceMenu menu;
        private float start,timeout=180,stuckFor;
        private int waypoint,completed;
        private string output;
        private Vector3 previous;
        private bool done;
        private RenderTexture verificationTarget, previousTarget;
        private bool offscreen, previousCameraEnabled, depthHall, contrastWing;
        private string pendingCapture;
        private Vector2[] route={new Vector2(10.8f,-9),new Vector2(10.8f,5.5f),new Vector2(19.7f,5.5f),new Vector2(19.7f,2.7f),new Vector2(25,2.7f),new Vector2(25,9),new Vector2(25,2.7f),new Vector2(19.7f,2.7f),new Vector2(19.7f,14.2f),new Vector2(25.5f,14.2f),new Vector2(25.5f,21.5f),new Vector2(1.8f,21.5f),new Vector2(-3.5f,21.5f),new Vector2(-3.5f,27),new Vector2(-3.5f,21.5f),new Vector2(-4.5f,21.5f),new Vector2(-4.5f,12),new Vector2(10.8f,12),new Vector2(10.8f,-9),new Vector2(0,-10)};
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-poolcore-qa")>=0) new GameObject("Runtime Verification").AddComponent<RuntimeVerification>();
        }
        private void Start()
        {
            Application.runInBackground=true;
            player=FindAnyObjectByType<FirstPersonController>(); menu=FindAnyObjectByType<ExperienceMenu>();
            output=Path.Combine(Application.persistentDataPath,"Verification");
            var args=Environment.GetCommandLineArgs();
            offscreen=Array.IndexOf(args,"-qa-offscreen")>=0;
            depthHall=Array.IndexOf(args,"-qa-depth-hall")>=0;
            if(depthHall) route=new[]{new Vector2(42.3f,22),new Vector2(36,22),new Vector2(42.3f,22),new Vector2(42.3f,32),new Vector2(74,32),new Vector2(74,0),new Vector2(58,0),new Vector2(58,7),new Vector2(58,10.8f),new Vector2(58,0),new Vector2(42.3f,0),new Vector2(42.3f,8.8f)};
            contrastWing=Array.IndexOf(args,"-qa-contrast-wing")>=0;
            if(contrastWing)route=new[]{new Vector2(40.5f,60),new Vector2(34.5f,61),new Vector2(34.5f,90),new Vector2(65.5f,90),new Vector2(65.5f,61),new Vector2(50,61),new Vector2(50,70),new Vector2(50,61),new Vector2(40.5f,61),new Vector2(40.5f,42),new Vector2(41.8f,37),new Vector2(42.3f,32),new Vector2(41.8f,37),new Vector2(40.5f,42)};
            for(int i=0;i<args.Length-1;i++)
            { if(args[i]=="-qa-output") output=args[i+1]; if(args[i]=="-qa-seconds" && float.TryParse(args[i+1],out float duration)) timeout=Mathf.Clamp(duration,15,600); }
            Directory.CreateDirectory(output); Application.logMessageReceived+=Log;
            start=Time.unscaledTime; previous=player.transform.position;
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
            if(offscreen) {
                previousTarget=player.view.targetTexture;previousCameraEnabled=player.view.enabled;
                // Match the sRGB game backbuffer. Reading a linear HDR target straight
                // into PNG made otherwise-correct lighting look artificially dark.
                verificationTarget=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);verificationTarget.Create();
                player.view.targetTexture=verificationTarget;player.view.enabled=false;
            }
        }
        private void Log(string message,string stack,LogType type)
        { if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) errors.Add(message+"\n"+stack); }
        private void Update()
        {
            if(done || Time.unscaledTime-start<3) return;
            menu.BeginAutomatedTest();
            if(timings.Count==0) { player.ResetToSpawn(); player.Teleport(contrastWing?new Vector3(40.5f,.05f,42):depthHall?new Vector3(42.3f,.05f,8.8f):new Vector3(0,.05f,-10)); previous=player.transform.position; Capture("start.png"); }
            timings.Add(Time.unscaledDeltaTime*1000);
            var position=new Vector2(player.transform.position.x,player.transform.position.z);
            var delta=route[waypoint]-position;
            if(delta.magnitude<0.15f)
            {
                completed++; waypoint=(waypoint+1)%route.Length;
                if(completed==6 || completed==14) Capture("waypoint-"+completed+".png");
                delta=route[waypoint]-position;
            }
            player.transform.rotation=Quaternion.identity;
            player.SimulateMovement(delta.normalized,false,Mathf.Min(Time.unscaledDeltaTime,0.05f));
            if(delta.sqrMagnitude>0.2f && Vector3.Distance(previous,player.transform.position)<0.0001f) stuckFor+=Time.unscaledDeltaTime; else stuckFor=0;
            previous=player.transform.position;
            if(delta.sqrMagnitude>0.05f) player.view.transform.rotation=Quaternion.Slerp(player.view.transform.rotation,Quaternion.LookRotation(new Vector3(delta.x,-0.07f,delta.y)),Time.unscaledDeltaTime*2);
            if(stuckFor>5) { errors.Add("Route stuck at "+player.transform.position+" towards "+route[waypoint]); Finish(); }
            else if(Time.unscaledTime-start>=timeout) Finish();
        }
        private void Capture(string file)
        {
            if(offscreen) pendingCapture=Path.Combine(output,file);
            else ScreenCapture.CaptureScreenshot(Path.Combine(output,file));
        }
        private void LateUpdate()
        {
            if(!offscreen || done || !player || !verificationTarget) return;
            // A hidden/batch player may skip the backbuffer. Explicit rendering measures
            // real scene and reflection work and produces inspectable QA screenshots.
            player.view.Render();
            if(pendingCapture==null) return;
            var active=RenderTexture.active;var read=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try {
                RenderTexture.active=verificationTarget;read.ReadPixels(new Rect(0,0,1920,1080),0,0);read.Apply();
                File.WriteAllBytes(pendingCapture,read.EncodeToPNG());pendingCapture=null;
            } finally {RenderTexture.active=active;Destroy(read);}
        }
        private void OnDestroy()
        {
            if(verificationTarget) {
                if(player) {player.view.targetTexture=previousTarget;player.view.enabled=previousCameraEnabled;}
                verificationTarget.Release();Destroy(verificationTarget);
            }
            Application.logMessageReceived-=Log;
        }
        private void Finish()
        {
            done=true; timings.Sort();
            var audio=player.GetComponent<PoolAudio>();
            int worldLabels=FindObjectsByType<TMPro.TextMeshPro>(FindObjectsInactive.Include).Length;
            if((depthHall || contrastWing) && (audio==null || audio.WadingVariants!=3 || audio.WadingSteps==0 || audio.DrySteps==0))errors.Add("Missing recorded audio or footstep triggers");
            if((depthHall || contrastWing) && worldLabels!=0)errors.Add("World labels remain");
            var report=new Report { unityVersion=Application.unityVersion,gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,frames=timings.Count,waypoints=completed,errors=errors.Count,seconds=Time.unscaledTime-start,result=errors.Count==0&&completed>=route.Length?"PASS":"FAIL" };
            report.drySteps=audio?audio.DrySteps:0;report.wadingSteps=audio?audio.WadingSteps:0;
            report.wadingVariants=audio?audio.WadingVariants:0;report.worldLabels=worldLabels;
            report.roomEmitters=FindObjectsByType<RoomSound>().Length;
            float sum=0; foreach(float t in timings) {sum+=t; if(t>100) report.stallsOver100ms++;}
            report.medianMs=Percentile(0.5f); report.p95Ms=Percentile(0.95f); report.p99Ms=Percentile(0.99f); report.averageFps=1000*timings.Count/Mathf.Max(1,sum);
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true)); File.WriteAllLines(Path.Combine(output,"errors.txt"),errors);
            Application.logMessageReceived-=Log; Application.Quit(report.result=="PASS"?0:1);
        }
        private float Percentile(float p) => timings.Count==0?0:timings[Mathf.Clamp(Mathf.FloorToInt((timings.Count-1)*p),0,timings.Count-1)];
    }
}
