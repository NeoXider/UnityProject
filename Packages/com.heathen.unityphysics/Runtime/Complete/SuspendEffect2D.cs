#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/2D Effects/Suspend")]
    public class SuspendEffect2D : ForceEffect2D
    {
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectTorque)
                subjectData.angularVelocity -= subjectData.angularVelocity * Mathf.Clamp01(strength);
        }

        public override void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectLinear)
                subjectData.velocity -= subjectData.velocity * Mathf.Clamp01(strength);
        }
    }
}

#endif
