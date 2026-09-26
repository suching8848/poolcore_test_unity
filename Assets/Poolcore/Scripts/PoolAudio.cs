using UnityEngine;

namespace Poolcore
{
    public sealed class PoolAudio : MonoBehaviour
    {
        public int DrySteps { get; private set; }
        public int WadingSteps { get; private set; }
        public int WadingVariants => wading==null?0:wading.Length;
        private AudioSource steps;
        private AudioClip dry;
        private AudioClip[] wading;
        private int lastWade=-1;
        private float nextStep;
        private Vector3 previous;
        private float distance;
        private CharacterController body;
        private readonly System.Collections.Generic.List<GameObject> emitters=new System.Collections.Generic.List<GameObject>();
        private void Start()
        {
            previous=transform.position;
            body=GetComponent<CharacterController>();
            steps=gameObject.AddComponent<AudioSource>(); steps.spatialBlend=0; steps.volume=0.21f;
            dry=MakeClip("Ceramic Footstep",0.23f,0);
            wading=Resources.LoadAll<AudioClip>("ImmersionAudio");
            wading=System.Array.FindAll(wading,c=>c.name=="Wade2" || c.name=="Wade4" || c.name=="Wade6");
            if(!GetComponent<RoomAcoustics>())gameObject.AddComponent<RoomAcoustics>();
            var bed=Resources.Load<AudioClip>("ImmersionAudio/Ventilation");
            foreach(var zone in FindObjectsByType<WaterZone>()) {
                if(zone.name=="Depth Hall Water")continue;
                var root=new GameObject(zone.name+" Room Audio");emitters.Add(root);
                RoomSound.Create(root.transform,"Distant Ventilation",zone.transform.position+new Vector3(0,3,3),bed,.075f,22,zone.transform.position.z);
                RoomSound.Create(root.transform,"Water Circulation",zone.transform.position+new Vector3(3,1,0),Resources.Load<AudioClip>("ImmersionAudio/Circulation"),.045f,15);
            }
        }
        private static AudioClip MakeClip(string name,float seconds,int kind)
        {
            const int rate=24000;var data=new float[Mathf.RoundToInt(seconds*rate)];
            var random=new System.Random(174);float low=0;
            for(int i=0;i<data.Length;i++) {
                float t=i/(float)rate;low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,.19f);
                data[i]=(low*.65f+Mathf.Sin(t*530)*.18f)*Mathf.Exp(-t*26)*Mathf.Min(1,t*650);
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        private void Update()
        {
            Vector3 delta=transform.position-previous; previous=transform.position;
            if(delta.sqrMagnitude>1) { distance=0; return; }
            float moved=new Vector2(delta.x,delta.z).magnitude;
            if(!body.isGrounded || moved<.0001f) { distance=0;return; }
            distance+=moved;
            float depth=WaterZone.DepthAt(transform.position);
            bool water=depth>.05f;
            if(distance<(water?.82f:1.15f) || Time.time<nextStep)return;
            distance=0;nextStep=Time.time+.28f;
            if(water) {
                if(wading.Length==0)return;
                int index=Random.Range(0,wading.Length);
                if(index==lastWade)index=(index+1)%wading.Length;
                WadingSteps++;lastWade=index;steps.pitch=Random.Range(.99f,1.01f);
                steps.PlayOneShot(wading[index],Mathf.Lerp(.20f,.36f,Mathf.Clamp01(depth/.6f)));
            }
            else { DrySteps++;steps.pitch=Random.Range(.9f,1.08f);steps.PlayOneShot(dry,.65f); }
        }
        private void OnDestroy()
        {
            if(dry) Destroy(dry);
            foreach(var emitter in emitters) if(emitter) Destroy(emitter);
        }
    }
}
