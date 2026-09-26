using UnityEngine;
namespace Poolcore
{
    // One listener-centred zone avoids competing overlapping spherical rooms.
    public sealed class RoomAcoustics : MonoBehaviour
    {
        private AudioReverbZone reverb;
        public static Vector2 Profile(Vector3 p)
        {
            float hall=Mathf.SmoothStep(0,1,Mathf.InverseLerp(34,41,p.x));
            float recess=Mathf.SmoothStep(0,1,Mathf.InverseLerp(32,34,p.x))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(37,41,p.x)))
                *Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,20,p.z))
                *(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(24,26,p.z)));
            float gallery=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(40,43,p.x)))
                *Mathf.SmoothStep(0,1,Mathf.InverseLerp(36,38,p.x))
                *Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,14,p.z))
                *(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(29,32,p.z)));
            var original=new Vector2(Mathf.Lerp(Mathf.Lerp(1.65f,2.9f,hall),.85f,Mathf.Max(recess,gallery)),Mathf.Lerp(-2100,-1500,hall)*(1-recess)+(-2400)*recess);
            float opening=Mathf.SmoothStep(0,1,Mathf.InverseLerp(55,61,p.z));
            var wing=new Vector2(Mathf.Lerp(.8f,3.1f,opening),Mathf.Lerp(-2400,-1500,opening));
            return Vector2.Lerp(original,wing,Mathf.SmoothStep(0,1,Mathf.InverseLerp(34,40,p.z)));

        }
        private void Start()
        {
            reverb=gameObject.AddComponent<AudioReverbZone>();reverb.reverbPreset=AudioReverbPreset.User;
            reverb.minDistance=4;reverb.maxDistance=5;reverb.room=-1700;reverb.roomHF=-1000;
            reverb.decayHFRatio=.6f;reverb.reflections=-2400;reverb.reflectionsDelay=.025f;
            reverb.reverbDelay=.035f;reverb.diffusion=92;reverb.density=95;
            var p=Profile(transform.position);reverb.decayTime=p.x;reverb.reverb=Mathf.RoundToInt(p.y);
        }
        private void Update()
        {
            var p=Profile(transform.position);float t=1-Mathf.Exp(-Time.unscaledDeltaTime*2);
            reverb.decayTime=Mathf.Lerp(reverb.decayTime,p.x,t);reverb.reverb=Mathf.RoundToInt(Mathf.Lerp(reverb.reverb,p.y,t));
        }
        private void OnDestroy(){if(reverb)Destroy(reverb);}
    }
}
