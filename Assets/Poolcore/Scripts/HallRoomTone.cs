using UnityEngine;
namespace Poolcore
{
    public sealed class HallRoomTone : MonoBehaviour
    {
        private void Start()
        {
            var bed=Resources.Load<AudioClip>("ImmersionAudio/Ventilation");
            foreach(float z in new[]{8f,26f})
                RoomSound.Create(transform,"Hall Ventilation Emitter",new Vector3(77.65f,2.6f,z),bed,.16f,44,z);
            RoomSound.Create(transform,"Hall Circulation Emitter",new Vector3(72.8f,.4f,27),Resources.Load<AudioClip>("ImmersionAudio/Circulation"),.07f,22);
        }
    }
}
