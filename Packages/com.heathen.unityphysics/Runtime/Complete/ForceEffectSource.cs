#if HE_SYSCORE

namespace HeathenEngineering.UnityPhysics
{
    public abstract class ForceEffectSource : HeathenBehaviour
    {
        public abstract void AddForce(PhysicsData subject, float sensitivity, bool useAngular, bool useLinear);
    }
}

#endif
