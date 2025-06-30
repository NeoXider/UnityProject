#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class ForceEffectField2D : ForceEffectSource2D
    {
        [SerializeField]
        private bool _isGlobal = false;
        public bool IsGlobal
        {
            get { return _isGlobal; }
            set
            {
                if (value)
                {
                    if (!API.ForceEffects2D.GlobalEffects.Contains(this))
                        API.ForceEffects2D.GlobalEffects.Add(this);
                }
                else
                {
                    if (API.ForceEffects2D.GlobalEffects.Contains(this))
                        API.ForceEffects2D.GlobalEffects.Remove(this);
                }
            }
        }

        public FloatReference strength = new FloatReference(1f);

        public FloatReference radius = new FloatReference(10f);
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 0);

        [Space]
        public List<ForceEffect2D> forceEffects = new();
        [HideInInspector]
        public bool hasTrigger = false;

        private void OnEnable()
        {
            var col = GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                hasTrigger = true;
            }

            if (_isGlobal)
            {
                if (!API.ForceEffects2D.GlobalEffects.Contains(this))
                    API.ForceEffects2D.GlobalEffects.Add(this);
            }
        }

        private void OnDisable()
        {
            API.ForceEffects2D.GlobalEffects.Remove(this);
        }

        public override void AddForce(Rigidbody2D subject, float sensitivity, bool useAngular, bool useLinear)
        {
            if (enabled)
            {
                if (IsGlobal)
                {
                    foreach (var e in forceEffects)
                    {
                        if (useLinear)
                            e.LinearEffect(SelfTransform.position, strength * sensitivity, subject);
                        if (useAngular)
                            e.AngularEffect(SelfTransform.position, strength * sensitivity, subject);
                    }
                }
                else
                {
                    var distance = Vector2.Distance(subject.transform.position, SelfTransform.position);
                    var delta = 1f - Mathf.Clamp01(distance / radius);
                    var curveStrength = strength * curve.Evaluate(delta);
                    if (curveStrength > 0)
                    {
                        foreach (var e in forceEffects)
                        {
                            if (useLinear)
                                e.LinearEffect(SelfTransform.position, curveStrength * sensitivity, subject);
                            if (useAngular)
                                e.AngularEffect(SelfTransform.position, curveStrength * sensitivity, subject);
                        }
                    }
                }
            }
        }
    }
}


#endif