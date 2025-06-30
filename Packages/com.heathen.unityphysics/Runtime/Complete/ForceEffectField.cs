#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class ForceEffectField : ForceEffectSource
    {
        [SerializeField]
        private bool _isGlobal = false;
        public bool IsGlobal {
            get { return _isGlobal; }
            set
            {
                if(value)
                {
                    if(!API.ForceEffects.GlobalEffects.Contains(this))
                        API.ForceEffects.GlobalEffects.Add(this);
                }
                else
                {
                    if (API.ForceEffects.GlobalEffects.Contains(this))
                        API.ForceEffects.GlobalEffects.Remove(this);
                }
            }
        }

        public FloatReference strength = new FloatReference(1f);

        public FloatReference radius = new FloatReference(10f);        
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 0);
        
        [Space]
        public List<ForceEffect> forceEffects = new List<ForceEffect>();
        [HideInInspector]
        public bool hasTrigger = false;

        private void OnEnable()
        {
            var col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                hasTrigger = true;
            }
            
            if (_isGlobal)
            {
                if (!API.ForceEffects.GlobalEffects.Contains(this))
                    API.ForceEffects.GlobalEffects.Add(this);
            }
        }

        private void OnDisable()
        {
            API.ForceEffects.GlobalEffects.Remove(this);
        }

        public override void AddForce(PhysicsData subject, float sinsativity, bool useAngular, bool useLinear)
        {
            if (enabled)
            {
                if (IsGlobal)
                {
                    foreach (var e in forceEffects)
                    {
                        if (useLinear)
                            e.LinearEffect(SelfTransform.position, strength * sinsativity, subject);
                        if (useAngular)
                            e.AngularEffect(SelfTransform.position, strength * sinsativity, subject);
                    }
                }
                else
                {
                    var distance = Vector3.Distance(subject.SelfTransform.position, SelfTransform.position);
                    var delta = 1f - Mathf.Clamp01(distance / radius);
                    var curveStrength = strength * curve.Evaluate(delta);
                    if (curveStrength > 0)
                    {
                        foreach (var e in forceEffects)
                        {
                            if (useLinear)
                                e.LinearEffect(SelfTransform.position, curveStrength * sinsativity, subject);
                            if (useAngular)
                                e.AngularEffect(SelfTransform.position, curveStrength * sinsativity, subject);
                        }
                    }
                }
            }
        }
    }
}


#endif