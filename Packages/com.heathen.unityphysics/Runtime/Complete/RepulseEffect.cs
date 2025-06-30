#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Repulse")]
    public class RepulseEffect : ForceEffect
    {
        public ForceMode mode;
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectTorque)
            {
                var dir = (subjectData.SelfTransform.position - origin).normalized;
                if (dir != Vector3.zero)
                    subjectData.AttachedRigidbody.AddTorque(API.Maths.TorqueToReachDirection(subjectData.AttachedRigidbody, dir) * strength, mode);
            }
        }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectLinear)
                subjectData.AttachedRigidbody.AddForce((subjectData.SelfTransform.position - origin).normalized * strength, mode);
        }
    }
}

#endif
