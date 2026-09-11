using UnityEngine;

namespace Poolcore
{
    [CreateAssetMenu(menuName = "Poolcore/Movement Settings")]
    public sealed class MovementSettings : ScriptableObject
    {
        [Min(0.1f)] public float walkSpeed = 2.4f;
        [Min(0.1f)] public float fastSpeed = 3.8f;
        [Range(0.01f, 0.4f)] public float sensitivity = 0.09f;
        [Range(55, 95)] public float fieldOfView = 75;
        public float gravity = -20f;
        public float respawnBelow = -8f;
    }
}
