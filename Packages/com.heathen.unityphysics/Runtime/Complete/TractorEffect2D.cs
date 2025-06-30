#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/2D Effects/Tractor")]
    public class TractorEffect2D : ForceEffect2D
    {
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectTorque)
            {
                var dir = (origin - subjectData.position).normalized;
                if (dir != Vector2.zero)
                    subjectData.AddTorque(API.Maths.TorqueToReachDirection2D(subjectData, dir) * strength);
            }
        }

        public override void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            if (EffectLinear)
                subjectData.AddForce((origin - subjectData.position) * strength);
        }
    }
}

#endif