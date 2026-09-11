using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Poolcore.Editor
{
    public static class ExperienceValidation
    {
        public static string ValidateRoute()
        {
            if(!Application.isPlaying) throw new Exception("Play Mode required");
            var player=UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            player.Release(); player.Teleport(new Vector3(0,0.06f,-10)); player.transform.rotation=Quaternion.identity;
            var report=new List<string>(); int failed=0;
            var route=new[]{new Vector2(10.8f,-9),new Vector2(10.8f,5.5f),new Vector2(19.7f,5.5f),new Vector2(19.7f,2.7f),new Vector2(25,2.7f),new Vector2(25,9),new Vector2(25,2.7f),new Vector2(19.7f,2.7f),new Vector2(19.7f,14.2f),new Vector2(25.5f,14.2f),new Vector2(25.5f,21.5f),new Vector2(1.8f,21.5f),new Vector2(-3.5f,21.5f),new Vector2(-3.5f,27),new Vector2(-3.5f,21.5f),new Vector2(-4.5f,21.5f),new Vector2(-4.5f,12),new Vector2(10.8f,12),new Vector2(10.8f,-9),new Vector2(0,-10)};
            foreach(var target in route)
            {
                int steps=0;
                while(steps++<1800)
                {
                    var delta=target-new Vector2(player.transform.position.x,player.transform.position.z);
                    if(delta.magnitude<0.09f) break;
                    player.SimulateMovement(delta.normalized,false,1f/60);
                }
                bool pass=steps<1800 && player.transform.position.y>-1.1f;
                report.Add((pass?"PASS":"FAIL")+" waypoint "+target+" at "+player.transform.position);
                if(!pass) { failed++; break; }
            }
            player.Teleport(new Vector3(0,-0.86f,4));
            bool submerged=WaterZone.DepthAt(player.transform.position)>0.5f;
            report.Add((submerged?"PASS":"FAIL")+" shallow water detected"); if(!submerged) failed++;
            float z=player.transform.position.z; for(int i=0;i<120;i++) player.SimulateMovement(Vector2.up,false,1f/60);
            float travelled=player.transform.position.z-z;
            bool slower=travelled>3 && travelled<3.6f; report.Add((slower?"PASS":"FAIL")+" wading 2 seconds = "+travelled.ToString("F3")+"m"); if(!slower) failed++;
            player.ResetToSpawn();
            var data=player.view.GetUniversalAdditionalCameraData(); var volume=UnityEngine.Object.FindAnyObjectByType<Volume>();
            bool post=data.renderPostProcessing && volume && volume.sharedProfile && UniversalRenderPipeline.asset.supportsHDR;
            report.Add((post?"PASS":"FAIL")+" HDR / camera / global volume"); if(!post) failed++;
            report.Add("Failures: "+failed); Directory.CreateDirectory("artifacts");
            string text=string.Join("\n",report); File.WriteAllText("artifacts/experience-validation.txt",text); return text;
        }
        public static string CaptureViews()
        {
            var camera=Camera.main; var pos=camera.transform.position; var rot=camera.transform.rotation;
            var points=new[]{new Vector3(0,1.7f,-10),new Vector3(10.4f,1.65f,6),new Vector3(25,1.65f,2.6f),new Vector3(-3.5f,1.65f,20),new Vector3(7,1.65f,21.5f)};
            var looks=new[]{new Vector3(0,1.3f,6),new Vector3(0,0,2),new Vector3(25,0.8f,10),new Vector3(-3.5f,1,28),new Vector3(24,1.65f,21.5f)};
            Directory.CreateDirectory("artifacts");
            try { for(int i=0;i<points.Length;i++) { camera.transform.position=points[i]; camera.transform.LookAt(looks[i]); SaveCamera(camera,"artifacts/poolrooms-"+i+".png"); } }
            finally { camera.transform.position=pos; camera.transform.rotation=rot; }
            return "Captured 5 views";
        }
        private static void SaveCamera(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,900,24); var previous=camera.targetTexture; var active=RenderTexture.active;
            try { camera.targetTexture=rt; camera.Render(); RenderTexture.active=rt;
                var tex=new Texture2D(1600,900,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1600,900),0,0); tex.Apply(); File.WriteAllBytes(path,tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
            } finally { camera.targetTexture=previous; RenderTexture.active=active; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
        }
    }
}
