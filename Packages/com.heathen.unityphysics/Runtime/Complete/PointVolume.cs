#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Expresses the volume assoceated with a mesh vertex or other point strucutre
    /// </summary>
    public struct PointVolume
    {
        public Vector3 point;
        public float volume;
    }
}

#endif