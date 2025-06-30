#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/2D Effects/Gravity")]
    public class GravityEffect2D : ForceEffect2D
    {
        public bool disableBodyGravity = true;

        public override void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        { }

        public override void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData)
        {
            var direction = (origin - subjectData.position).normalized * strength;

            if (disableBodyGravity)
                subjectData.gravityScale = 0;

            subjectData.AddForce(direction * subjectData.mass);
        }
    }
}

#endif
