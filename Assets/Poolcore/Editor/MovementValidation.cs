using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Poolcore.Editor
{
    public static class MovementValidation
    {
        public static string Run()
        {
            if (!Application.isPlaying) throw new Exception("Run validation in Play Mode");
            var p = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            var results = new List<string>();
            void Check(string name, bool pass, string detail)
            { results.Add((pass ? "PASS " : "FAIL ") + name + ": " + detail); }
            void Place(Vector3 pos) { p.Teleport(pos); p.transform.rotation=Quaternion.identity; }
            void Walk(Vector2 input, int frames, bool fast=false)
            { for (int i=0;i<frames;i++) p.SimulateMovement(input,fast,1f/60); }
            Place(new Vector3(0,0.05f,-7)); Walk(Vector2.up,60);
            float straight=p.transform.position.z+7;
            Place(new Vector3(0,0.05f,-7)); Walk(Vector2.one,60);
            float diagonal=Vector2.Distance(new Vector2(0,-7),new Vector2(p.transform.position.x,p.transform.position.z));
            Check("normalized diagonal",Mathf.Abs(straight-diagonal)<0.02f,$"straight={straight:F3}, diagonal={diagonal:F3}");
            Check("walk speed",Mathf.Abs(straight-2.4f)<0.02f,straight.ToString("F3"));
            Place(new Vector3(0,0.05f,-7)); Walk(Vector2.up,60,true);
            Check("fast walk",Mathf.Abs(p.transform.position.z+7-3.8f)<0.03f,p.transform.position.ToString());
            Place(new Vector3(11,0.05f,-7)); Walk(Vector2.right,120);
            Check("wall blocks",p.transform.position.x<11.76f && p.transform.position.x>11.5f,p.transform.position.ToString());
            Place(new Vector3(0,-0.75f,3)); Walk(Vector2.down,120);
            Check("basin stair ascent",p.transform.position.y>-0.1f && p.transform.position.z<-1,p.transform.position.ToString());
            Place(new Vector3(0,0.05f,-2)); Walk(Vector2.up,150);
            Check("basin descent",Mathf.Abs(p.transform.position.y+0.8f)<0.1f,p.transform.position.ToString());
            Place(new Vector3(-8,0.05f,-8)); Walk(Vector2.up,130);
            Check("ramp ascent",p.transform.position.y>0.6f,p.transform.position.ToString());
            Place(new Vector3(0,-20,0)); Walk(Vector2.zero,1);
            Check("fall recovery",Vector3.Distance(p.transform.position,new Vector3(0,0.08f,-7))<0.1f,p.transform.position.ToString());
            p.ResetToSpawn();
            Directory.CreateDirectory("artifacts");
            var report=string.Join("\n",results); File.WriteAllText("artifacts/movement-validation.txt",report);
            return report;
        }
    }
}
