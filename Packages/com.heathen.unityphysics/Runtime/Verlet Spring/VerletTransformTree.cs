#if HE_SYSCORE

using UnityEngine;
using System.Collections.Generic;
using System;
using HeathenEngineering.UnityPhysics.API;
using Unity.Mathematics;

namespace HeathenEngineering.UnityPhysics
{
    [Serializable]
    public class VerletTransformTree
    {
        /// <summary>
        /// The root of the tree
        /// </summary>
        public Transform root;
        /// <summary>
        /// The settings to be applied to this tree
        /// </summary>
        public VerletTransformTreeSettings settings;
        /// <summary>
        /// The child nodes of the root of this tree
        /// </summary>
        public List<VerletTransformNode> nodes = new List<VerletTransformNode>();
        /// <summary>
        /// The list of transforms to ignore when constructing child nodes
        /// </summary>
        public List<Transform> ignoreList = new List<Transform>();

        private float distance;

        /// <summary>
        /// Scans the transforms from the root down and constructs <see cref="VerletParticle"/>s
        /// </summary>
        public void RegisterNodes()
        {
            if (settings == null)
                return;

            if (Application.isPlaying)
                ResetNodes();

            nodes.Clear();

            AddNodes(root, null, 0);

            foreach (var n in nodes)
            {
                n.weight = distance == 0 ? 0 : n.distance / distance;
            }
        }

        private void AddNodes(Transform node, VerletTransformNode parent, float boneDistance)
        {
            VerletTransformNode n = new VerletTransformNode();
            n.target = node;
            n.parent = parent;

            if (node != null)
            {
                n.position = node.position;
                n.prevPosition = node.position;
                n.prevTimestep = Time.deltaTime;
                n.initLocalPosition = node.localPosition;
                n.initLocalRotation = node.localRotation;
            }

            if (parent != null)
            {
                n.length = math.distance(parent.target.position, n.target.position);
                boneDistance += n.length;
                n.distance = boneDistance;
                distance = Mathf.Max(distance, boneDistance);
            }

            nodes.Add(n);

            if (node != null)
            {
                foreach (Transform c in node)
                {
                    if (!ignoreList.Contains(c))
                    {
                        AddNodes(c, n, boneDistance);
                    }
                }
            }
        }

        private bool ResolveCollisionPenetration(VerletTransformNode node, LayerMask layerMask)
        {
            if (Physics.SphereCast(new Ray(node.parent.position, math.normalize(node.position - node.parent.position)), settings.collision.Evaluate(node.weight), out RaycastHit hit, math.distance(node.position, node.parent.position), layerMask))
            {
                Vector3 pointOnLine = NearestPointOnLineSegment(node.parent.position, node.position, hit.point);
                float3 penetrationHeading = (hit.point - pointOnLine);
                var penetrationShift = -math.normalize(penetrationHeading) * (settings.collision.Evaluate(node.weight) - math.length(penetrationHeading));

                //node.prevPosition += penetrationShift;
                node.position += penetrationShift;
                return true;
            }
            return false;
        }

        public void Update(float time)
        {
            var constantVelocity = settings.constantAcceleration;
            var layerMask = settings.collisionLayers;

            foreach (var n in nodes)
            {
                if (n.target != root)
                {
                    var damping = math.clamp(settings.damping.Evaluate(n.weight), 0f, 1f);
                    var elasticity = math.max(0f, settings.elasticity.Evaluate(n.weight));
                    var stiffness = math.clamp(settings.stiffness.Evaluate(n.weight), 0f, 1f);
                    var drag = math.clamp(settings.drag.Evaluate(n.weight), 0f, 1f);
                    var falloffAngle = math.clamp(settings.angle.Evaluate(n.weight), 0f, 360f);
                    var collisionRadius = math.max(0f, settings.collision.Evaluate(n.weight));
                    float3 restPosition = n.parent.target.TransformPoint(n.initLocalPosition);
                    var displacement = restPosition - n.position;

                    //Hooke's Law
                    //This expresses the elasticity constant in terms of newtons so we use an assumed mass of 0.01 for human friendly numbers
                    var elasticVelocity = Maths.ElasticityAcceleration(0.01f, displacement, elasticity);

                    var pos = n.position;
                    n.position = Maths.TimeCorrectedVerletIntegration(n.position, n.prevPosition, (1f - drag), constantVelocity + n.addedForce + elasticVelocity, (1f - damping), time, n.prevTimestep);
                    n.prevPosition = pos;
                    n.prevTimestep = time;
                    n.addedForce = Vector3.zero;

                    if (stiffness > 0)
                    {                        
                        // Apply parent stiffness
                        displacement = restPosition - n.position;
                        var distance = math.length(displacement);
                        var limit = n.length * (1f - stiffness) * 2f;

                        if (distance > limit)
                        {
                            n.position += displacement * ((distance - limit) / distance);
                            displacement = restPosition - n.position;
                        }

                        n.parent.addedForce += -displacement * stiffness;
                    }

                    // Apply falloff angle
                    if (falloffAngle > 0)
                    {
                        float angle = Quaternion.Angle(Quaternion.LookRotation(n.position - n.parent.position), Quaternion.LookRotation(n.parent.target.TransformDirection(n.initLocalPosition)));
                        if(angle > falloffAngle)
                        {
                            //Clamp to our angle limit
                            n.position = math.lerp(restPosition, n.position, falloffAngle / angle);
                        }
                    }

                    // Apply actualLength
                    n.position = n.parent.target.position + ((Vector3)n.position - n.parent.target.position).normalized * n.length;

                    if (collisionRadius > 0)
                        ResolveCollisionPenetration(n, layerMask);

                    n.target.localRotation = n.initLocalRotation;
                    var targetDirection = n.position - n.parent.position;                    
                    var rot = Quaternion.FromToRotation(n.parent.target.TransformDirection(n.initLocalPosition), targetDirection);
                    n.target.rotation = rot * n.target.rotation;

                    // Apply the final position to the target
                    n.target.position = n.position;
                }
                else
                {
                    // Handle root node
                    n.position = n.target.position;
                    n.target.localRotation = n.initLocalRotation;
                    n.addedForce = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Adds a force to all nodes in the tree
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(float3 force)
        {
            foreach (var n in nodes)
                n.addedForce += force;
        }

        /// <summary>
        /// Adds a force to all nodes in the tree originating from the position provided and dampend by the length of the node over the node distance from the force position
        /// </summary>
        /// <param name="forceMagnitude">The strength of the force</param>
        /// <param name="position">The force's point of origin</param>
        public void AddForceAtPosition(float forceMagnitude, float3 position)
        {
            foreach (var n in nodes)
            {
                var d = math.distance(position, n.target.position);
                var l = n.parent != null ? math.distance(n.parent.target.position, n.target.position) : 0;
                if (l > 0 && d < l)
                {
                    float3 pPos = n.target.position;
                    float3 force = math.normalize(pPos - position) * (forceMagnitude * (d / l));
                    n.addedForce += force;
                }
            }
        }

        /// <summary>
        /// Return all nodes to a rested state
        /// </summary>
        public void ResetNodes()
        {
            if (nodes.Count > 0)
            {
                foreach (var n in nodes)
                {
                    n.target.localPosition = n.initLocalPosition;
                    n.target.localRotation = n.initLocalRotation;
                    n.position = n.target.position;
                    n.prevPosition = n.target.position;
                    n.prevTimestep = Time.deltaTime;
                }
            }
        }

        private float3 NearestPointOnLineSegment(float3 lineStart, float3 lineEnd, float3 subject)
        {
            var line = (lineEnd - lineStart);
            var length = math.length(line);
            line = math.normalize(line);

            var subjectHeading = subject - lineStart;
            var dot = Vector3.Dot(subjectHeading, line);
            dot = Mathf.Clamp(dot, 0f, length);
            return lineStart + line * dot;
        }
    }
}

#endif