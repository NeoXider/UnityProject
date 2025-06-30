#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Used to return the surface depth of a dynamic surface at a given world point such as an ocean surface
    /// </summary>
    public class SurfaceTool : HeathenBehaviour
    {
        public float SurfaceDensity = 1.293f;
        public float SubsurfaceDensity = 1000f;

        /// <summary>
        /// This should be overriden to return the depth given at this world point
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public virtual float SurfaceDepth(Vector3 worldPoint)
        {
            return SelfTransform.position.y - worldPoint.y;
        }

        /// <summary>
        /// This should be overriden to return the normal of the surfaces at, above or below this world point
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public virtual Vector3 SurfaceNormal(Vector3 worldPoint)
        {
            var a = worldPoint + new Vector3(0, SurfaceDepth(worldPoint), 0);
            var b = a + Vector3.right * 1;
            b = b + new Vector3(0, SurfaceDepth(b), 0);
            var c = b - Vector3.forward * 1;
            c = c + new Vector3(0, SurfaceDepth(c), 0);
            return API.MeshTools.CalculateNormal(a, b, c);
        }
    }
}

#endif
