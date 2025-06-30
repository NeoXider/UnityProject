#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics.API
{
    public static class Buoyancy
    {
        /// <summary>
        /// Determines the effect strength of buoyancy on the PhysicsData object given the indicated surface.
        /// </summary>
        /// <remarks>
        /// This is a complex calculation that accounts for each vertex of the physics data hull.
        /// </remarks>
        /// <param name="data">The Physics object to test, the volume mesh of this object will be used in the calculation</param>
        /// <param name="surface">The surface to calculate buoyancy for</param>
        /// <param name="gravity">The magnitude of gravity working on the system</param>
        /// <param name="submergedPoints">Returns the collection of submerged hull points, this can be used to apply detailed forces to the object</param>
        /// <param name="volume">The calculated displacement volume</param>
        /// <returns>The strength of the force buoyancy would apply</returns>
        public static float Effect(PhysicsData data, SurfaceTool surface, float gravity, out List<PointVolume> submergedPoints, out float volume)
        {
            volume = 0f;
            var vertices = data.HullGeometry.vertices;
            var triangles = data.HullGeometry.triangles;

            submergedPoints = new List<PointVolume>();

            for (int i = 0; i < data.HullGeometry.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];

                var tV = data.SelfTransform.TransformVector(p1);
                //Debug.Log(tV);
                var p1d = surface.SurfaceDepth(data.SelfTransform.position + data.SelfTransform.TransformVector(p1));
                var p2d = surface.SurfaceDepth(data.SelfTransform.position + data.SelfTransform.TransformVector(p2));
                var p3d = surface.SurfaceDepth(data.SelfTransform.position + data.SelfTransform.TransformVector(p3));

                if (p1d > 0 && p2d > 0 && p3d > 0)
                {
                    //voxel is completely submerged
                    var val = MeshTools.SignedVolumeOfTriangle(p1, p2, p3);
                    submergedPoints.Add(new PointVolume() { point = p1, volume = val * 0.3333f });
                    submergedPoints.Add(new PointVolume() { point = p2, volume = val * 0.3333f });
                    submergedPoints.Add(new PointVolume() { point = p3, volume = val * 0.3333f });
                    volume += val;
                }
                else
                {
                    if (p1d > 0 && p2d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s1 = (p3 - p1).normalized * p1d * (1f / data.SelfTransform.localScale.y);
                        var s2 = (p3 - p2).normalized * p2d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(p1, p2, s2);
                        volume += val;
                        submergedPoints.Add(new PointVolume() { point = p1, volume = val * 0.3333f });
                        val = MeshTools.SignedVolumeOfTriangle(s1, p2, s2);
                        submergedPoints.Add(new PointVolume() { point = p2, volume = val * 0.3333f });
                        volume += val;
                    }
                    else if (p3d > 0 && p2d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s3 = (p1 - p3).normalized * p3d * (1f / data.SelfTransform.localScale.y);
                        var s2 = (p1 - p2).normalized * p2d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(p3, p2, s2);
                        volume += val;
                        submergedPoints.Add(new PointVolume() { point = p2, volume = val * 0.3333f });
                        val = MeshTools.SignedVolumeOfTriangle(s3, p2, s2);
                        submergedPoints.Add(new PointVolume() { point = p3, volume = val * 0.3333f });
                        volume += val;
                    }
                    else if (p3d > 0 && p1d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s3 = (p2 - p3).normalized * p3d * (1f / data.SelfTransform.localScale.y);
                        var s1 = (p2 - p1).normalized * p1d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(p3, p1, s1);
                        submergedPoints.Add(new PointVolume() { point = p1, volume = val * 0.3333f });
                        volume += val;
                        val = MeshTools.SignedVolumeOfTriangle(s3, p1, s1);
                        submergedPoints.Add(new PointVolume() { point = p3, volume = val * 0.3333f });
                        volume += val;
                    }
                    else if (p1d > 0)
                    {
                        //single point submerged form a new triangle
                        var s2 = (p2 - p1).normalized * p1d * (1f / data.SelfTransform.localScale.y);
                        var s3 = (p3 - p1).normalized * p1d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(p1, s2, s3);
                        volume += val;
                        submergedPoints.Add(new PointVolume() { point = p1, volume = val * 0.3333f });
                    }
                    else if (p2d > 0)
                    {
                        //single point submerged form a new triangle
                        var s1 = (p1 - p2).normalized * p2d * (1f / data.SelfTransform.localScale.y);
                        var s3 = (p3 - p2).normalized * p2d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(s1, p2, s3);
                        volume += val;
                        submergedPoints.Add(new PointVolume() { point = p2, volume = val * 0.3333f });
                    }
                    else if (p3d > 0)
                    {
                        //single point submerged form a new triangle
                        var s1 = (p1 - p3).normalized * p3d * (1f / data.SelfTransform.localScale.y);
                        var s2 = (p2 - p3).normalized * p3d * (1f / data.SelfTransform.localScale.y);
                        var val = MeshTools.SignedVolumeOfTriangle(s1, s2, p3);
                        volume += val;
                        submergedPoints.Add(new PointVolume() { point = p3, volume = val * 0.3333f });
                    }
                }
            }

            if (volume > 0)
            {
                var submergedVolume = Mathf.Abs(volume) * data.SelfTransform.localScale.x * data.SelfTransform.localScale.y * data.SelfTransform.localScale.z;
                return submergedVolume * surface.SubsurfaceDensity * gravity;
            }
            else
                return 0;
        }

        /// <summary>
        /// Determines the effect strength of buoyancy on the PhysicsData object given the indicated surface.
        /// </summary>
        /// <remarks>
        /// This is a simpler solution that approximates displacement based on the bounding box of the object
        /// </remarks>
        /// <param name="data">The Physics object tto test, the volume of this object will be used along with its bounding points</param>
        /// <param name="surface">The surface to calculate buoyancy for</param>
        /// <param name="gravity">The magnitude of gravity working on the system</param>
        /// <param name="volume">The calculated displacement volume</param>
        /// <returns>The strength of the force buoyancy would apply</returns>
        public static float Effect(PhysicsData data, SurfaceTool surface, float gravity, out float volume)
        {
            volume = 0f;

            Vector3[] boundPoints = data.GetBoundCorners();

            for (int i = 0; i < 8; i++)
            {
                var dVal = surface.SurfaceDepth(boundPoints[i]);
                if (dVal > 0)
                {
                    //Very rough approximation of area represented as submerged by find the max area represented by this point '0.125' and finding the ratio of that below the surface
                    volume += (0.125f * Mathf.Clamp01(dVal / data.Bounds.extents.y)) * data.volume;
                }
            }

            if (volume > 0)
            {
                return (volume * surface.SubsurfaceDensity * gravity);
            }
            else
                return 0;
        }

        /// <summary>
        /// Finds the displacement volume of a mesh that is below the surface line of a <see cref="SurfaceTool"/>
        /// </summary>
        /// <param name="root">The root of the mesh, used to account for position, rotation and scale</param>
        /// <param name="mesh">The hull to be calculated</param>
        /// <param name="scale">The scale of the mesh relative to the root, this should usually be (1, 1, 1)</param>
        /// <param name="surface">The surface to calculate against</param>
        /// <returns></returns>
        public static float Displacement(Transform root, Mesh mesh, Vector3 scale, SurfaceTool surface)
        {
            var volume = 0f;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];

                var p1d = surface.SurfaceDepth(root.TransformVector(p1));
                var p2d = surface.SurfaceDepth(root.TransformVector(p2));
                var p3d = surface.SurfaceDepth(root.TransformVector(p3));

                if (p1d > 0 && p2d > 0 && p3d > 0)
                {
                    //voxel is completely submerged
                    volume += MeshTools.SignedVolumeOfTriangle(p1, p2, p3);
                }
                else
                {
                    if (p1d > 0 && p2d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s1 = (p3 - p1).normalized * p1d * (1f / scale.y);
                        var s2 = (p3 - p2).normalized * p2d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(p1, p2, s2);
                        volume += MeshTools.SignedVolumeOfTriangle(s1, p2, s2);
                    }
                    else if (p3d > 0 && p2d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s3 = (p1 - p3).normalized * p3d * (1f / scale.y);
                        var s2 = (p1 - p2).normalized * p2d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(p3, p2, s2);
                        volume += MeshTools.SignedVolumeOfTriangle(s3, p2, s2);
                    }
                    else if (p3d > 0 && p1d > 0)
                    {
                        //2 point submerged form a set of 2 new triangles
                        var s3 = (p2 - p3).normalized * p3d * (1f / scale.y);
                        var s1 = (p2 - p1).normalized * p1d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(p3, p1, s1);
                        volume += MeshTools.SignedVolumeOfTriangle(s3, p1, s1);
                    }
                    else if (p1d > 0)
                    {
                        //single point submerged form a new triangle
                        var s2 = (p2 - p1).normalized * p1d * (1f / scale.y);
                        var s3 = (p3 - p1).normalized * p1d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(p1, s2, s3);
                    }
                    else if (p2d > 0)
                    {
                        //single point submerged form a new triangle
                        var s1 = (p1 - p2).normalized * p2d * (1f / scale.y);
                        var s3 = (p3 - p2).normalized * p2d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(s1, p2, s3);
                    }
                    else if (p3d > 0)
                    {
                        //single point submerged form a new triangle
                        var s1 = (p1 - p3).normalized * p3d * (1f / scale.y);
                        var s2 = (p2 - p3).normalized * p3d * (1f / scale.y);
                        volume += MeshTools.SignedVolumeOfTriangle(s1, s2, p3);
                    }
                }
            }
            return Mathf.Abs(volume) * scale.x * scale.y * scale.z;
        }

        /// <summary>
        /// Calculates the force applied given a submerged volume
        /// </summary>
        /// <param name="displacementVolume">The volume of the environment displaced</param>
        /// <param name="density">The density of the environment e.g. water is around 1000</param>
        /// <param name="gravityMagnitude">The magnitude of the effect of gravity ... earth would be aprox 9.8f</param>
        /// <returns>The strength of the force buoyancy would apply</returns>
        public static float Effect(float displacementVolume, float density, float gravityMagnitude) => displacementVolume * density * gravityMagnitude;
    }
}
#endif