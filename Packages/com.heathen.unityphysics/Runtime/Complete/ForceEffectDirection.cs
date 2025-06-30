#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class ForceEffectDirection : ForceEffectSource
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
                    if (!API.ForceEffects.GlobalEffects.Contains(this))
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

        public FloatReference reach = new FloatReference(10f);
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
                        //var planePoint = new Plane(SelfTransform.forward, SelfTransform.position).ClosestPointOnPlane(subject.SelfTransform.position);

                        if (useLinear)
                            e.LinearEffect(subject.SelfTransform.position + SelfTransform.forward, strength * sinsativity, subject);
                        if (useAngular)
                            e.AngularEffect(subject.SelfTransform.position + SelfTransform.forward, strength * sinsativity, subject);
                    }
                }
                else
                {
                    var planePoint = new Plane(SelfTransform.forward, SelfTransform.position).ClosestPointOnPlane(subject.SelfTransform.position);
                    var distance = Vector3.Distance(subject.SelfTransform.position, planePoint);
                    var delta = 1f - Mathf.Clamp01(distance / reach);
                    var curveStrength = strength * curve.Evaluate(delta);
                    if (curveStrength > 0)
                    {
                        foreach (var e in forceEffects)
                        {
                            if (useLinear)
                                e.LinearEffect(subject.SelfTransform.position + SelfTransform.forward, curveStrength * sinsativity, subject);
                            if (useAngular)
                                e.AngularEffect(subject.SelfTransform.position + SelfTransform.forward, curveStrength * sinsativity, subject);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var matrix = Gizmos.matrix;
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(SelfTransform.position, SelfTransform.position + (SelfTransform.forward * strength.Value));
            Vector3 right = Quaternion.LookRotation(SelfTransform.forward) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(SelfTransform.forward) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(SelfTransform.position + (SelfTransform.forward * strength.Value), right * .25f);
            Gizmos.DrawRay(SelfTransform.position + (SelfTransform.forward * strength.Value), left * .25f);
        }
    }
}


#endif