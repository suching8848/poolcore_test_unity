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
        private readonly Vector2[] route={new Vector2(10.8f,-9),new Vector2(10.8f,5.5f),new Vector2(19.7f,5.5f),new Vector2(19.7f,2.7f),new Vector2(25,2.7f),new Vector2(25,9),new Vector2(25,2.7f),new Vector2(19.7f,2.7f),new Vector2(19.7f,14.2f),new Vector2(25.5f,14.2f),new Vector2(25.5f,21.5f),new Vector2(1.8f,21.5f),new Vector2(-3.5f,21.5f),new Vector2(-3.5f,27),new Vector2(-3.5f,21.5f),new Vector2(-4.5f,21.5f),new Vector2(-4.5f,12),new Vector2(10.8f,12),new Vector2(10.8f,-9),new Vector2(0,-10)};
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
            for(int i=0;i<args.Length-1;i++)
            { if(args[i]=="-qa-output") output=args[i+1]; if(args[i]=="-qa-seconds" && float.TryParse(args[i+1],out float duration)) timeout=Mathf.Clamp(duration,15,600); }
            Directory.CreateDirectory(output); Application.logMessageReceived+=Log;
            start=Time.unscaledTime; previous=player.transform.position;
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
        }
        private void Log(string message,string stack,LogType type)
        { if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) errors.Add(message+"\n"+stack); }
        private void Update()
        {
            if(done || Time.unscaledTime-start<3) return;
            menu.BeginAutomatedTest();
            if(timings.Count==0) { player.ResetToSpawn(); previous=player.transform.position; ScreenCapture.CaptureScreenshot(Path.Combine(output,"start.png")); }
            timings.Add(Time.unscaledDeltaTime*1000);
            var position=new Vector2(player.transform.position.x,player.transform.position.z);
            var delta=route[waypoint]-position;
            if(delta.magnitude<0.15f)
            {
                completed++; waypoint=(waypoint+1)%route.Length;
                if(completed==6 || completed==14) ScreenCapture.CaptureScreenshot(Path.Combine(output,"waypoint-"+completed+".png"));
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
        private void Finish()
        {
            done=true; timings.Sort();
            var report=new Report { unityVersion=Application.unityVersion,gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,frames=timings.Count,waypoints=completed,errors=errors.Count,seconds=Time.unscaledTime-start,result=errors.Count==0&&completed>=route.Length?"PASS":"FAIL" };
            float sum=0; foreach(float t in timings) {sum+=t; if(t>100) report.stallsOver100ms++;}
            report.medianMs=Percentile(0.5f); report.p95Ms=Percentile(0.95f); report.p99Ms=Percentile(0.99f); report.averageFps=1000*timings.Count/Mathf.Max(1,sum);
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true)); File.WriteAllLines(Path.Combine(output,"errors.txt"),errors);
            Application.logMessageReceived-=Log; Application.Quit(report.result=="PASS"?0:1);
        }
        private float Percentile(float p) => timings.Count==0?0:timings[Mathf.Clamp(Mathf.FloorToInt((timings.Count-1)*p),0,timings.Count-1)];
    }
}
