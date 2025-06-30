#if HE_SYSCORE

using MIConvexHull;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Double precision Vertex structure used in various mesh and large scale calculations.
    /// </summary>
    public class Vertex : IVertex
    {
        public double[] Position { get; set; }
        public Vertex(double x, double y, double z)
        {
            Position = new double[3] { x, y, z };
        }
        public Vertex(Vector3 ver)
        {
            Position = new double[3] { ver.x, ver.y, ver.z };
        }
        public Vector3 ToVec()
        {
            return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        }
    }
}

#endif
