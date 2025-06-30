#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Gravity")]
    public class GravityEffect : ForceEffect
    {
        public bool disableBodyGravity = true;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        { }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            var direction = (origin - subjectData.SelfTransform.position).normalized * strength;

            if (disableBodyGravity)
                subjectData.AttachedRigidbody.useGravity = false;

            subjectData.AttachedRigidbody.AddForce(direction * subjectData.Mass);
        }
    }
}

#endif
