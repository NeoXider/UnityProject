#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Interface for Force Effect objects
    /// </summary>
    public interface IForceEffect
    {
        /// <summary>
        /// Apply a linear effect to a <see cref="PhysicsData"/> object
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="strength"></param>
        /// <param name="subjectData"></param>
        void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData);
        /// <summary>
        /// Apply an angular effect to a <see cref="PhysicsData"/> object
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="strength"></param>
        /// <param name="subjectData"></param>
        void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData);
    }
}

#endif