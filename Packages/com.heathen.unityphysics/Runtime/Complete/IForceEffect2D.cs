#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public interface IForceEffect2D
    {
        /// <summary>
        /// Apply a linear effect to a <see cref="Rigidbody2D"/> object
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="strength"></param>
        /// <param name="subjectData"></param>
        void LinearEffect(Vector2 origin, float strength, Rigidbody2D subjectData);
        /// <summary>
        /// Apply an angular effect to a <see cref="Rigidbody2D"/> object
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="strength"></param>
        /// <param name="subjectData"></param>
        void AngularEffect(Vector2 origin, float strength, Rigidbody2D subjectData);
    }
}

#endif