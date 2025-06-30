#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Suspend")]
    public class SuspendEffect : ForceEffect
    {
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectTorque)
                subjectData.AttachedRigidbody.angularVelocity -= subjectData.AttachedRigidbody.angularVelocity * Mathf.Clamp01(strength);
        }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectLinear)
                subjectData.AttachedRigidbody.velocity -= subjectData.AttachedRigidbody.velocity * Mathf.Clamp01(strength);
        }
    }
}

#endif
