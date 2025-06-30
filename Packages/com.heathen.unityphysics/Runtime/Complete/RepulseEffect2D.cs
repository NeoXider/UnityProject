#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/2D Effects/Repulse")]
    public class RepulseEffect2D : ForceEffect2D
    {
        public ForceMode2D mode;
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectTorque)
            {
                var dir = (subjectData.position - origin).normalized;
                if (dir != Vector2.zero)
                    subjectData.AddTorque(API.Maths.TorqueToReachDirection2D(subjectData, dir) * strength, mode);
            }
        }

        public override void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectLinear)
                subjectData.AddForce((subjectData.position - origin).normalized * strength, mode);
        }
    }
}

#endif
