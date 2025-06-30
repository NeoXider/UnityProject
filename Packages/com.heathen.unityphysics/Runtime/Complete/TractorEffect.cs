#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Tractor")]
    public class TractorEffect : ForceEffect
    {
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectTorque)
            {
                var dir = (origin - subjectData.SelfTransform.position).normalized;
                if (dir != Vector3.zero)
                    subjectData.AttachedRigidbody.AddTorque(API.Maths.TorqueToReachDirection(subjectData.AttachedRigidbody, dir) * strength, ForceMode.VelocityChange);
            }
        }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectLinear)
                subjectData.AttachedRigidbody.AddForce((origin - subjectData.SelfTransform.position) * strength, ForceMode.VelocityChange);
        }
    }
}

#endif