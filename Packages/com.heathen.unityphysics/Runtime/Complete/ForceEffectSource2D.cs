#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public abstract class ForceEffectSource2D : HeathenBehaviour
    {
        public abstract void AddForce(Rigidbody2D subject, float sensitivity, bool useAngular, bool useLinear);
    }
}

#endif
