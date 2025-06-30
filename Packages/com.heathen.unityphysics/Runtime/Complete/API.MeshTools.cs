#if HE_SYSCORE

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics.API
{
    public static class MeshTools
    {
        /// <summary>
        /// Gets the combined vertex of a collection of mesh objects and returns a new convex mesh which contains them
        /// </summary>
        /// <param name="volumes"></param>
        /// <returns></returns>
        public static Mesh ConvexHull(IEnumerable<MeshFilter> volumes)
        {
            List<Vector3> vertexList = new List<Vector3>();
            foreach (var f in volumes)
            {
                vertexList.AddRange(f.mesh.vertices);
            }

            return ConvexHull(vertexList);
        }

        /// <summary>
        /// <para>Calculate a convex hull given the input meshes</para>
        /// <para>This transforms the node positions to be zeroed on the position and rotation of the provided root</para>
        /// </summary>
        /// <param name="root"></param>
        /// <param name="volumes"></param>
        /// <returns></returns>
        public static Mesh ConvexHull(Transform root, IEnumerable<MeshFilter> volumes)
        {
            if (root == null)
            {
                Debug.LogError($"{nameof(MeshTools.ConvexHull)} parameter root is null, no convex hull can be created!");
                return null;
            }

            if(volumes == null)
            {
                Debug.LogError($"{nameof(MeshTools.ConvexHull)} parameter volume is null, no convex hull can be created!");
                return null;
            }

            List<Vector3> vertexList = new List<Vector3>();
            foreach (var f in volumes)
            {
                if (f == null)
                {
                    Debug.LogWarning($"{nameof(MeshTools.ConvexHull)} parameter volumes contains a null MeshFilter, the null element will be skipped");
                }
                else if (f.sharedMesh == null)
                {
                    Debug.LogWarning($"{nameof(MeshTools.ConvexHull)} parameter volumes contains a MeshFilter({f.gameObject.name}) with a null sharedMesh, the null element will be skipped");
                }
                else
                {
                    var vTrans = f.GetComponent<Transform>();
                    if (vTrans == null)
                    {
                        Debug.LogWarning($"{nameof(MeshTools.ConvexHull)} parameter volumes contains a MeshFilter({f.gameObject.name}) without a transform, the null element will be skipped");
                    }
                    else
                    {
                        foreach (var v in f.sharedMesh.vertices)
                        {
                            var rootLocal = root.InverseTransformPoint(vTrans.TransformPoint(v));
                            vertexList.Add(rootLocal);
                        }
                    }
                }
            }

            return ConvexHull(vertexList);
        }

        /// <summary>
        /// <para>Calculate a convex hull given the input meshes</para>
        /// </summary>
        /// <param name="volumes"></param>
        /// <returns></returns>
        public static Mesh ConvexHull(IEnumerable<Mesh> volumes)
        {
            List<Vector3> vertexList = new List<Vector3>();
            foreach (var f in volumes)
            {
                vertexList.AddRange(f.vertices);
            }

            return ConvexHull(vertexList);
        }

        /// <summary>
        /// <para>Calculate a convex hull given the input meshes</para>
        /// </summary>
        /// <param name="volumes"></param>
        /// <returns></returns>
        public static Mesh ConvexHull(MeshFilter mesh)
        {
            return ConvexHull(mesh.mesh.vertices);
        }

        /// <summary>
        /// Leverages MIConvexHull to generate a convex hull around an array of points representing the vertex of a subject mesh
        /// </summary>
        /// <param name="vertexList"></param>
        /// <returns></returns>
        public static Mesh ConvexHull(IEnumerable<Vector3> vertexList)
        {
            Mesh m = new Mesh();
            m.name = "ConvexHullMesh";
            List<int> triangles = new List<int>();

            var vertices = vertexList.Select(x => new Vertex(x)).ToList();

            var result = MIConvexHull.ConvexHull.Create(vertices);
            m.vertices = result.Points.Select(x => x.ToVec()).ToArray();
            var xxx = result.Points.ToList();

            foreach (var face in result.Faces)
            {
                triangles.Add(xxx.IndexOf(face.Vertices[0]));
                triangles.Add(xxx.IndexOf(face.Vertices[1]));
                triangles.Add(xxx.IndexOf(face.Vertices[2]));
            }

            m.triangles = triangles.ToArray();
            m.RecalculateNormals();
            return m;
        }

        /// <summary>
        /// Creates a convex mesh from the provided mesh and then performs the volume calculation on the result
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="scale"></param>
        public static float VolumeOfConvexMesh(Mesh mesh, Vector3 scale)
        {
            var conMesh = ConvexHull(mesh.vertices);
            return VolumeOfMesh(conMesh, scale);
        }

        /// <summary>
        /// Find the volume of a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static float VolumeOfMesh(Mesh mesh, Vector3 scale)
        {
            var volume = 0f;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }
            return Mathf.Abs(volume) * scale.x * scale.y * scale.z;
        }

        /// <summary>
        /// Calculates the normal of a triangle
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var s1 = p2 - p1;
            var s2 = p3 - p1;
            var perp = Vector3.Cross(s1, s2);
            return perp.normalized;
        }

        /// <summary>
        /// Get the surface area of a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static float SurfaceArea(Mesh mesh)
        {
            var sum = 0.0;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]] - corner;
                Vector3 b = vertices[triangles[i + 2]] - corner;

                sum += Vector3.Cross(a, b).magnitude;
            }

            return System.Convert.ToSingle(sum / 2.0);
        }

        /// <summary>
        /// Get the surface area of a mesh in a given direction e.g. the leading cross section
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float FacingSurfaceArea(Mesh mesh, Vector3 direction)
        {
            direction = direction.normalized;
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;

            var sum = 0.0;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]] - corner;
                Vector3 b = vertices[triangles[i + 2]] - corner;

                var projection = Vector3.Dot(Vector3.Cross(b, a), direction);
                if (projection > 0f)
                    sum += projection;
            }

            return System.Convert.ToSingle(sum / 2.0);
        }

        /// <summary>
        /// Finds the area and volume of a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="scale"></param>
        /// <param name="surfaceArea"></param>
        /// <param name="volume"></param>
        public static void GetAreaAndVolume(Mesh mesh, Vector3 scale, out float surfaceArea, out float volume)
        {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;

            var sum = 0.0;
            volume = 0;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 corner = vertices[triangles[i]];
                Vector3 a = vertices[triangles[i + 1]];
                Vector3 b = vertices[triangles[i + 2]];

                volume += SignedVolumeOfTriangle(corner, a, b);

                sum += Vector3.Cross(a - corner, b - corner).magnitude;
            }

            volume = Mathf.Abs(volume) * scale.x * scale.y * scale.z;
            surfaceArea = System.Convert.ToSingle(sum / 2.0);
        }

        /// <summary>
        /// Find the volume of a triangle this is used when calculating the volume of a mesh
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float v321 = p3.x * p2.y * p1.z;
            float v231 = p2.x * p3.y * p1.z;
            float v312 = p3.x * p1.y * p2.z;
            float v132 = p1.x * p3.y * p2.z;
            float v213 = p2.x * p1.y * p3.z;
            float v123 = p1.x * p2.y * p3.z;
            return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }
    }
}


#endif