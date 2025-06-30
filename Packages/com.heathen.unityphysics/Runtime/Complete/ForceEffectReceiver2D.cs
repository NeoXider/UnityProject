#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ForceEffectReceiver2D : HeathenBehaviour
    {
        public BoolReference useAngular = new(false);
        public BoolReference useLinear = new(false);
        [Tooltip("How subjectable is this receiver to force effect fields")]
        public FloatReference sensitivity = new(1f);
        [HideInInspector]
        public Rigidbody2D data;

        private void Start()
        {
            data = GetComponent<Rigidbody2D>();
        }

        [Tooltip("List of triggered effectors e.g. effectors that are not managed as global")]
        public List<ForceEffectSource2D> Effectors = new();
        [Tooltip("List of effect fields to ignore")]
        public List<ForceEffectSource2D> IgnoreList = new();

        private void FixedUpdate()
        {
            if (data != null)
            {
                foreach (var field in API.ForceEffects2D.GlobalEffects)
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            var field = other.gameObject.GetComponentInChildren<ForceEffectSource2D>();
            if (field != null && !Effectors.Contains(field))
            {
                Effectors.Add(field);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var field = other.gameObject.GetComponentInChildren<ForceEffectSource2D>();
            if (field != null && Effectors.Contains(field))
            {
                Effectors.Remove(field);
            }
        }
    }
}


#endif