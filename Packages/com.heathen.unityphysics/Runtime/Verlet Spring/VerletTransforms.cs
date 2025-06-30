#if HE_SYSCORE

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace HeathenEngineering.UnityPhysics
{
    public class VerletTransforms : HeathenBehaviour
    {
        public bool showGizmo = true;
        /// <summary>
        /// A list of the managed <see cref="VerletTransformTree"/>s
        /// </summary>
        public List<VerletTransformTree> transformHierarchies = new List<VerletTransformTree>();

        private void Start()
        {
            RegisterTrees();
        }

        private void LateUpdate()
        {
            float time = Time.deltaTime;

            foreach (var tree in transformHierarchies)
                tree.Update(time);
        }

        /// <summary>
        /// Registers the <see cref="transformHierarchies"/> node objects
        /// </summary>
        [ContextMenu("Register Nodes")]
        public void RegisterTrees()
        {
            foreach (var tree in transformHierarchies)
            {
                tree.RegisterNodes();
            }
        }

        /// <summary>
        /// Adds a force to all trees
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            foreach (var n in transformHierarchies)
                n.AddForce(force);
        }

        /// <summary>
        /// Adds a force at a given position to all trees
        /// </summary>
        /// <param name="forceMagnitude"></param>
        /// <param name="position"></param>
        public void AddForceAtPosition(float forceMagnitude, Vector3 position)
        {
            foreach (var n in transformHierarchies)
                n.AddForceAtPosition(forceMagnitude, position);
        }

        /// <summary>
        /// Adds a force to a specific bone tree based on tree index
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="force"></param>
        public void AddForce(int treeIndex, Vector3 force)
        {
            if (treeIndex < transformHierarchies.Count && treeIndex >= 0)
                transformHierarchies[treeIndex].AddForce(force);
            else
            {
                if (transformHierarchies == null || transformHierarchies.Count == 0)
                {
                    Debug.LogError("Attempted to add force to tree at index " + treeIndex.ToString() + "; this Physics Bone has no trees.");
                }
                else
                {
                    Debug.LogError("Attempted to add force to tree index " + treeIndex.ToString() + "; this Physics Bone contains " + transformHierarchies.Count.ToString() + "Bone Trees. Index must be a value from 0 to " + (transformHierarchies.Count - 1).ToString());
                }
            }
        }

        /// <summary>
        /// Returns the bone tree at the indicated index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VerletTransformTree GetTree(int index)
        {
            if (index < transformHierarchies.Count && index >= 0)
                return transformHierarchies[index];
            else
            {
                if (transformHierarchies == null || transformHierarchies.Count == 0)
                {
                    Debug.LogWarning("Attempted to get tree at index " + index.ToString() + "; this Physics Bone has no trees.");
                    return null;
                }
                else
                {
                    Debug.LogWarning("Attempted to get tree at index " + index.ToString() + "; this Physics Bone contains " + transformHierarchies.Count.ToString() + "Bone Trees. Index must be a value from 0 to " + (transformHierarchies.Count - 1).ToString());
                    return null;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!enabled || !showGizmo)
                return;

            foreach (var tree in transformHierarchies)
            {
                if (Application.isEditor && !Application.isPlaying && transform.hasChanged)
                {
                    tree.RegisterNodes();
                }

                foreach (var n in tree.nodes)
                {
                    if (n.parent != null)
                    {
                        Gizmos.color = new Color(1f, 1f, 0.75f, 1f);
                        Vector3 direction = n.parent.target.position - n.target.position;
                        float length = direction.magnitude;

                        if (length == 0)
                            continue;

                        Matrix4x4 cTrans = Matrix4x4.TRS(n.target.position + (direction * 0.5f), Quaternion.LookRotation(direction, Vector3.up), new Vector3(Mathf.Clamp(0.05f * length, 0.01f, .1f), Mathf.Clamp(0.05f * length, 0.01f, .1f), length));
                        Matrix4x4 pTrans = Gizmos.matrix;
                        Gizmos.matrix *= cTrans;
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        Gizmos.matrix = pTrans;

                        var colRad = tree.settings.collision.Evaluate(n.weight);

                        if (colRad > 0)
                        {
                            DrawWireCapsule(n.target.position + (direction * 0.5f), Quaternion.LookRotation(Vector3.Cross(direction, Vector3.forward), direction), colRad, length + (colRad * 2f), new Color(.5f, 1f, 0.5f, 1f));
                        }
                    }
                }
            }
        }

        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);

            }
        }
    }
}


#endif