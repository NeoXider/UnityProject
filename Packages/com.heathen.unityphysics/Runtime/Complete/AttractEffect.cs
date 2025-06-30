#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Attract")]
    public class AttractEffect : ForceEffect
    {
        public ForceMode mode;
        public bool EffectTorque = false;
        public bool EffectLinear = false;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectTorque)
            { 
                var dir = (origin - subjectData.SelfTransform.position).normalized;
                if (dir != Vector3.zero)
                    subjectData.AttachedRigidbody.AddTorque(API.Maths.TorqueToReachDirection(subjectData.AttachedRigidbody, dir) * strength, mode);
            }
        }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectLinear)
            {
                var heading = origin - subjectData.SelfTransform.position;
                subjectData.AttachedRigidbody.AddForce(heading.normalized * strength, mode);
            }
        }
    }
}

#endif
