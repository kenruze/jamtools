using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JamTools;

namespace JamTools
{
    public partial class FootStepZone : MonoBehaviour
    {

        public EffectType footstepEffect;
        public int priority = 0;

        [Tooltip("For water surface")]
        public bool overridePositionY;

        [Header("Exposed")]
        public Collider trigger;

        public bool IsInside(Vector3 position)
        {
            if(trigger.bounds.Contains(position))
            {
                var boxes = GetComponents<BoxCollider>();
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (Geometry.IsInside(boxes[i], position))
                        return true;
                }
                var spheres = GetComponents<SphereCollider>();
                for (int i = 0; i < spheres.Length; i++)
                {
                    if (Geometry.IsInside(spheres[i], position))
                        return true;
                }
            }
            return false;
        }
    }


    //STATIC CLASS
    public partial class FootStepZone : MonoBehaviour
    {
        static List<FootStepZone> s_allZones;

        public static FootStepZone GetZone(Vector3 position)
        {
            if (s_allZones == null)
            {
                s_allZones = new List<FootStepZone>();
                s_allZones.AddRange(FindObjectsOfType<FootStepZone>());
            }
            FootStepZone zone = null;
            int priority = int.MinValue;

            for (int i = 0; i < s_allZones.Count; i++)
            {
                if (s_allZones[i] == null)
                {
                    s_allZones.RemoveAt(i);
                    --i;
                    continue;
                }

                if (s_allZones[i].IsInside(position))
                {
                    if (s_allZones[i].priority > priority)
                    {
                        zone = s_allZones[i];
                        priority = s_allZones[i].priority;
                    }
                }
            }
            return zone;
        }
    }
}