#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(PhysicsData))]
    public class ForceEffectReceiver : HeathenBehaviour
    {
        public BoolReference useAngular = new(false);
        public BoolReference useLinear = new(false);
        [Tooltip("How subjectable is this receiver to force effect fields")]
        public FloatReference sensitivity = new(1f);
        [HideInInspector]
        public PhysicsData data;

        private void Start()
        {
            data = GetComponent<PhysicsData>();
        }

        [Tooltip("List of triggered effectors e.g. effectors that are not managed as global")]
        public List<ForceEffectSource> Effectors = new();
        [Tooltip("List of effect fields to ignore")]
        public List<ForceEffectSource> IgnoreList = new();

        private void FixedUpdate()
        {
            if(data != null)
            {
                foreach(var field in API.ForceEffects.GlobalEffects)
                {
                    if (!IgnoreList.Contains(field))
                        field.AddForce(data, sensitivity.Value, useAngular.Value, useLinear.Value);
                }
            }

            if (data != null && Effectors != null && Effectors.Count > 0)
            {
                foreach (var field in Effectors)
                {
                    if (field.enabled && field.gameObject.activeInHierarchy && sensitivity.Value != 0)
                    {
                        if (!IgnoreList.Contains(field))
                            field.AddForce(data, sensitivity.Value, useAngular.Value, useLinear.Value);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var field = other.gameObject.GetComponentInChildren<ForceEffectSource>();
            if(field != null && !Effectors.Contains(field))
            {
                Effectors.Add(field);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var field = other.gameObject.GetComponentInChildren<ForceEffectSource>();
            if (field != null && Effectors.Contains(field))
            {
                Effectors.Remove(field);
            }
        }
    }
}


#endif