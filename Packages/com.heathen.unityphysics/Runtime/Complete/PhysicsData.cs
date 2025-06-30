#if HE_SYSCORE
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// <para>Calculates useful physics data based on a source rigidbody and an input hull.</para>
    /// <para>This enables the use of complex <see cref="PhysKitMaths"/> calculations and cashes rigidbody data such as velocities. If this is present it should be used over the <see cref="Rigidbody.velocity"/> and similar calls</para>
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsData : HeathenBehaviour
    {
        /// <summary>
        /// If true additional debug data will be drawn in the editor when this object is selected.
        /// </summary>
        public bool debug = false;
        public Mesh customHullMesh;
        [SerializeField]
        [HideInInspector]
        private Mesh hullGeometry;
        /// <summary>
        /// The mesh to use as the hull of this object
        /// </summary>
        public Mesh HullGeometry
        {
            get => hullGeometry;
            private set
            {
                hullGeometry = value;
            }
        }
        /// <summary>
        /// The mass in Kg of this object
        /// </summary>
        public float Mass => AttachedRigidbody.mass;
        public float LinearDragCoefficient => AttachedRigidbody.drag;
        public float AngularDragCoefficient => AttachedRigidbody.angularDrag;
        public List<MeshFilter> ignoredMesh = new List<MeshFilter>();
        public Vector3 LinearDragEffect
        {
            get
            {
                return -LinearDragCoefficient * LinearVelocity.normalized * LinearVelocity.sqrMagnitude;
            }
        }
        public Vector3 AngularDragEffect
        {
            get
            {
                return -AngularDragCoefficient * AngularVelocity.normalized * AngularVelocity.sqrMagnitude;
            }
        }
        /// <summary>
        /// How much mass per how much volume ... more simply how dense on average
        /// </summary>
        public float Density => Mass / volume;
        /// <summary>
        /// The volume of the hull
        /// </summary>
        public float volume;
        /// <summary>
        /// The surface area of the hull
        /// </summary>
        public float area;
        /// <summary>
        /// The cross section surface area along the X axis
        /// </summary>
        public float xCrossSection;
        /// <summary>
        /// The cross section surface area along the Y axis
        /// </summary>
        public float yCrossSection;
        /// <summary>
        /// The cross section surface area along the Z axis
        /// </summary>
        public float zCrossSection;
        /// <summary>
        /// The heading of the object
        /// </summary>
        public Vector3 LinearHeading => AttachedRigidbody.velocity.normalized;
        /// <summary>
        /// The speed of the object
        /// </summary>
        public float LinearSpeed => AttachedRigidbody.velocity.magnitude;
        /// <summary>
        /// The velocity of the object
        /// </summary>
        public Vector3 LinearVelocity => AttachedRigidbody.velocity;
        /// <summary>
        /// The rotational heading of the object
        /// </summary>
        public Vector3 AngularHeading => AttachedRigidbody.angularVelocity.normalized;
        /// <summary>
        /// The rotational speed of the object
        /// </summary>
        public float AngularSpeed => AttachedRigidbody.angularVelocity.magnitude;
        /// <summary>
        /// The rotaitonal velocity of the object
        /// </summary>
        public Vector3 AngularVelocity => AttachedRigidbody.angularVelocity;
        /// <summary>
        /// Returns the bounds of the hull
        /// </summary>
        public Bounds Bounds
        {
            get { return HullGeometry != null ? HullGeometry.bounds : new Bounds(); }
        }
        /// <summary>
        /// Returns the attached rigidbody
        /// </summary>
        public Rigidbody AttachedRigidbody
        {
            get;
            private set;
        }
        
        private void Awake()
        {
            AttachedRigidbody = GetComponentInChildren<Rigidbody>();
            List<MeshFilter> meshes = new List<MeshFilter>();
            GetComponentsInChildren(true, meshes);
            meshes.RemoveAll((m) => { return ignoredMesh.Contains(m); });
            RegisterGeometry(meshes);
            
        }

        /// <summary>
        /// Creates a Hull Geometry based on a set of input MeshFilters
        /// </summary>
        /// <param name="sourceGeometry"></param>
        public void RegisterGeometry(IEnumerable<MeshFilter> sourceGeometry)
        {
            if (HullGeometry != null)
                DestroyImmediate(HullGeometry);

            if (customHullMesh != null)
                HullGeometry = API.MeshTools.ConvexHull(customHullMesh.vertices);
            else
                HullGeometry = API.MeshTools.ConvexHull(SelfTransform, sourceGeometry);

            volume = 0;
            area = 0;

            API.MeshTools.GetAreaAndVolume(HullGeometry, SelfTransform.localScale, out area, out volume);
            xCrossSection = API.MeshTools.FacingSurfaceArea(HullGeometry, Vector3.right);
            yCrossSection = API.MeshTools.FacingSurfaceArea(HullGeometry, Vector3.up);
            zCrossSection = API.MeshTools.FacingSurfaceArea(HullGeometry, Vector3.forward);
        }

        /// <summary>
        /// Finds the cross section of the hull in a given direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public float CrossSection(Vector3 direction)
        {
            var normal = direction.normalized;
            normal = SelfTransform.InverseTransformDirection(normal);
            return (xCrossSection * Mathf.Abs(normal.x)) + (yCrossSection * Mathf.Abs(normal.y)) + (zCrossSection * Mathf.Abs(normal.z));
        }

        /// <summary>
        /// Returns the corners of the hull bounds
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetBoundCorners()
        {
            var v1 = SelfTransform.TransformPoint(Bounds.center + new Vector3(Bounds.extents.x, Bounds.extents.y, Bounds.extents.z));
            var v2 = SelfTransform.TransformPoint(Bounds.center + new Vector3(Bounds.extents.x, Bounds.extents.y, -Bounds.extents.z));
            var v3 = SelfTransform.TransformPoint(Bounds.center + new Vector3(Bounds.extents.x, -Bounds.extents.y, Bounds.extents.z));
            var v4 = SelfTransform.TransformPoint(Bounds.center + new Vector3(-Bounds.extents.x, Bounds.extents.y, Bounds.extents.z));
            var v5 = SelfTransform.TransformPoint(Bounds.center + new Vector3(Bounds.extents.x, -Bounds.extents.y, -Bounds.extents.z));
            var v6 = SelfTransform.TransformPoint(Bounds.center + new Vector3(-Bounds.extents.x, Bounds.extents.y, -Bounds.extents.z));
            var v7 = SelfTransform.TransformPoint(Bounds.center + new Vector3(-Bounds.extents.x, -Bounds.extents.y, Bounds.extents.z));
            var v8 = SelfTransform.TransformPoint(Bounds.center + new Vector3(-Bounds.extents.x, -Bounds.extents.y, -Bounds.extents.z));

            Vector3[] boundPoints = new Vector3[] { v1, v2, v3, v4, v5, v6, v7, v8 };

            return boundPoints;
        }

        private void OnDrawGizmos()
        {
            if (debug)
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                RegisterGeometry(meshFilters);
                AttachedRigidbody = GetComponentInChildren<Rigidbody>();

                var matrix = Gizmos.matrix;
                Gizmos.color = Color.yellow;
                var rotMatrix = Matrix4x4.TRS(SelfTransform.position, SelfTransform.rotation, SelfTransform.lossyScale);
                Gizmos.matrix = rotMatrix;
                Gizmos.DrawWireCube(Bounds.center, Bounds.size);
                Gizmos.matrix = matrix;

                var p = GetBoundCorners();
                foreach(var v in p)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(v, 0.1f);
                }

                Gizmos.color = Color.red;
                Gizmos.DrawLine(SelfTransform.position, SelfTransform.position + LinearVelocity);
                Vector3 right = Quaternion.LookRotation(LinearHeading) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(LinearHeading) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(SelfTransform.position + LinearVelocity, right * .25f);
                Gizmos.DrawRay(SelfTransform.position + LinearVelocity, left * .25f);

            }
        }
    }
}

#endif