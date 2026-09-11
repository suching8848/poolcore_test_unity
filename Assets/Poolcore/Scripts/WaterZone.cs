using System.Collections.Generic;
using UnityEngine;

namespace Poolcore
{
    public sealed class WaterZone : MonoBehaviour
    {
        public Vector2 min, max;
        public float surface=-0.28f;
        private static readonly List<WaterZone> Zones=new List<WaterZone>();
        private void OnEnable() { if(!Zones.Contains(this)) Zones.Add(this); }
        private void OnDisable() { Zones.Remove(this); }
        public static float DepthAt(Vector3 feet)
        {
            float depth=0;
            foreach(var zone in Zones)
                if(zone && feet.x>zone.min.x && feet.x<zone.max.x && feet.z>zone.min.y && feet.z<zone.max.y)
                    depth=Mathf.Max(depth,zone.surface-feet.y);
            return Mathf.Clamp(depth,0,1);
        }
    }
}
