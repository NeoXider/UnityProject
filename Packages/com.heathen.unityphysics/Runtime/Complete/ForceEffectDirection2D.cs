#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class ForceEffectDirection2D : ForceEffectSource2D
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

        public FloatReference reach = new FloatReference(10f);
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 0);

        [Space]
        public List<ForceEffect2D> forceEffects = new();
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
                        //var planePoint = new Plane(SelfTransform.forward, SelfTransform.position).ClosestPointOnPlane(subject.SelfTransform.position);

                        if (useLinear)
                            e.LinearEffect(subject.transform.position + SelfTransform.forward, strength * sensitivity, subject);
                        if (useAngular)
                            e.AngularEffect(subject.transform.position + SelfTransform.forward, strength * sensitivity, subject);
                    }
                }
                else
                {
                    var planePoint = new Plane(SelfTransform.forward, SelfTransform.position).ClosestPointOnPlane(subject.transform.position);
                    var distance = Vector3.Distance(subject.transform.position, planePoint);
                    var delta = 1f - Mathf.Clamp01(distance / reach);
                    var curveStrength = strength * curve.Evaluate(delta);
                    if (curveStrength > 0)
                    {
                        foreach (var e in forceEffects)
                        {
                            if (useLinear)
                                e.LinearEffect(subject.transform.position + SelfTransform.forward, curveStrength * sensitivity, subject);
                            if (useAngular)
                                e.AngularEffect(subject.transform.position + SelfTransform.forward, curveStrength * sensitivity, subject);
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