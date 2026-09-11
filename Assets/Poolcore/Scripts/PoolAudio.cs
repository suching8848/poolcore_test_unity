using UnityEngine;

namespace Poolcore
{
    public sealed class PoolAudio : MonoBehaviour
    {
        private AudioSource steps;
        private AudioClip dry, wet, ambience;
        private Vector3 previous;
        private float distance;
        private CharacterController body;
        private readonly System.Collections.Generic.List<GameObject> emitters=new System.Collections.Generic.List<GameObject>();
        private void Start()
        {
            previous=transform.position;
            body=GetComponent<CharacterController>();
            steps=gameObject.AddComponent<AudioSource>(); steps.spatialBlend=0; steps.volume=0.21f;
            dry=MakeClip("Ceramic Footstep",0.23f,0); wet=MakeClip("Shallow Water Step",0.55f,1);
            ambience=MakeClip("Pool Room Atmosphere",12f,2);
            foreach(var zone in FindObjectsByType<WaterZone>())
            {
                var emitter=new GameObject(zone.name+" Water Ambience"); emitters.Add(emitter); emitter.transform.position=zone.transform.position+Vector3.up*0.4f;
                var source=emitter.AddComponent<AudioSource>(); source.clip=ambience; source.loop=true; source.volume=0.3f; source.spatialBlend=1;
                source.minDistance=3; source.maxDistance=20; source.rolloffMode=AudioRolloffMode.Linear; source.dopplerLevel=0; source.Play();
                var reverb=emitter.AddComponent<AudioReverbZone>(); reverb.reverbPreset=AudioReverbPreset.StoneCorridor; reverb.minDistance=3; reverb.maxDistance=12;
            }
        }
        private static AudioClip MakeClip(string name,float seconds,int kind)
        {
            const int rate=24000; var data=new float[Mathf.RoundToInt(seconds*rate)];
            var random=new System.Random(174+kind); float low=0,band=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate; float noise=(float)random.NextDouble()*2-1;
                low=Mathf.Lerp(low,noise,kind==0?0.19f:0.04f); band=Mathf.Lerp(band,noise,0.5f);
                if(kind==0) data[i]=(low*0.65f+Mathf.Sin(t*530)*0.18f)*Mathf.Exp(-t*26)*Mathf.Min(1,t*650);
                else if(kind==1) data[i]=(low*1.5f+(band-low)*0.13f)*Mathf.Exp(-t*7)*Mathf.Min(1,t*100);
                else
                {
                    float pulse=(0.6f+0.25f*Mathf.Sin(t*Mathf.PI/3)+0.15f*Mathf.Cos(t*Mathf.PI/2));
                    data[i]=low*0.7f*pulse+Mathf.Sin(t*Mathf.PI*2*54)*0.014f;
                    float local=t%2.4f;
                    data[i]+=Mathf.Sin(local*(2100-500*local))*Mathf.Exp(-local*22)*0.05f;
                    // Seam cross-fade to zero prevents clicks on the repeating ambient bed.
                    data[i]*=Mathf.Min(1,Mathf.Min(t,seconds-t)*4);
                }
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false); clip.SetData(data,0); return clip;
        }
        private void Update()
        {
            Vector3 delta=transform.position-previous; previous=transform.position;
            if(delta.sqrMagnitude>1) { distance=0; return; }
            distance+=new Vector2(delta.x,delta.z).magnitude;
            if(distance<1.15f || !body.isGrounded) return;
            distance=0; bool water=WaterZone.DepthAt(transform.position)>0.05f;
            steps.pitch=Random.Range(0.9f,1.08f); steps.PlayOneShot(water?wet:dry,water?0.8f:0.65f);
            if(water) Shader.SetGlobalVector("_PoolFootRipple",new Vector4(transform.position.x,transform.position.z,Time.time,1));
        }
        private void OnDestroy()
        {
            if(dry) Destroy(dry); if(wet) Destroy(wet); if(ambience) Destroy(ambience);
            foreach(var emitter in emitters) if(emitter) Destroy(emitter);
        }
    }
}
