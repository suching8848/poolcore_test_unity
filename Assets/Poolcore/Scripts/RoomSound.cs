using UnityEngine;

namespace Poolcore
{
    // A physical emitter with inexpensive, smoothed wall occlusion.
    public sealed class RoomSound : MonoBehaviour
    {
        private AudioSource source;
        private AudioLowPassFilter filter;
        private Transform listener;
        private float gain, target=1, nextProbe;
        public static void Create(Transform parent,string name,Vector3 position,AudioClip clip,float volume,float range,float offset=0)
        {
            if(!clip)return;
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;
            var sound=go.AddComponent<RoomSound>();sound.gain=volume;
            sound.source=go.AddComponent<AudioSource>();var a=sound.source;
            a.clip=clip;a.loop=true;a.playOnAwake=false;a.spatialBlend=1;a.volume=0;
            a.minDistance=2;a.maxDistance=range;a.rolloffMode=AudioRolloffMode.Linear;a.dopplerLevel=0;a.reverbZoneMix=.45f;
            sound.filter=go.AddComponent<AudioLowPassFilter>();sound.filter.cutoffFrequency=7000;
            var ears=Object.FindAnyObjectByType<AudioListener>();if(ears)sound.listener=ears.transform;
            a.time=offset%clip.length;a.Play();
        }
        private void Update()
        {
            if(!source||!listener)return;
            if(Time.unscaledTime>=nextProbe) {
                nextProbe=Time.unscaledTime+.2f;
                var delta=transform.position-listener.position;
                bool blocked=Physics.Raycast(listener.position,delta.normalized,out var hit,delta.magnitude,~(1<<4),QueryTriggerInteraction.Ignore)
                    && !hit.transform.IsChildOf(listener.root);
                target=blocked?.22f:1;
            }
            float t=1-Mathf.Exp(-Time.unscaledDeltaTime*2);
            source.volume=Mathf.Lerp(source.volume,gain*target,t);
            filter.cutoffFrequency=Mathf.Lerp(filter.cutoffFrequency,target<.5f?650:7000,t);
        }
    }
}
