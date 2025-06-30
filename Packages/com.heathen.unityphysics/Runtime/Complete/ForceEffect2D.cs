#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public abstract class ForceEffect2D : ScriptableObject, IForceEffect2D
    {
        public abstract void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData);

        public abstract void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData);
    }
}

#endif