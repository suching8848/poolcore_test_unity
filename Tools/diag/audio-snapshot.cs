var sources=UnityEngine.Object.FindObjectsByType<UnityEngine.AudioSource>(UnityEngine.FindObjectsSortMode.None);
var rows=new System.Collections.Generic.List<string>();
foreach(var s in sources)rows.Add(s.name+" clip="+(s.clip?s.clip.name:"one-shot")+" playing="+s.isPlaying+" volume="+s.volume+" filter="+(s.GetComponent<UnityEngine.AudioLowPassFilter>()?s.GetComponent<UnityEngine.AudioLowPassFilter>().cutoffFrequency:0));
return new {rows,listeners=UnityEngine.Object.FindObjectsByType<UnityEngine.AudioListener>(UnityEngine.FindObjectsSortMode.None).Length,zones=UnityEngine.Object.FindObjectsByType<UnityEngine.AudioReverbZone>(UnityEngine.FindObjectsSortMode.None).Length,worldText=UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>(UnityEngine.FindObjectsSortMode.None).Length};
