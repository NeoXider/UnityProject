#if HE_SYSCORE

using UnityEngine;
using System.Collections.Generic;
using System;
using HeathenEngineering.UnityPhysics.API;
using Unity.Mathematics;

namespace HeathenEngineering.UnityPhysics
{
    [Serializable]
    public class VerletTransformTree2D
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
        public List<VerletTransformNode2D> nodes = new();
        /// <summary>
        /// The list of transforms to ignore when constructing child nodes
        /// </summary>
        public List<Transform> ignoreList = new();

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

        private void AddNodes(Transform node, VerletTransformNode2D parent, float boneDistance)
        {
            VerletTransformNode2D n = new();
            n.target = node;
            n.parent = parent;

            if (node != null)
            {
                n.position = new(node.position.x, node.position.y);
                n.prevPosition = new(node.position.x, node.position.y);
                n.prevTimestep = Time.deltaTime;
                n.initLocalPosition = new(node.localPosition.x, node.localPosition.y);
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

        private bool ResolveCollisionPenetration(VerletTransformNode2D node, LayerMask layerMask)
        {
            //if (Physics.SphereCast(new Ray(node.parent.position, math.normalize(node.position - node.parent.position)), settings.collision.Evaluate(node.weight), out RaycastHit hit, math.distance(node.position, node.parent.position), layerMask))
            var hit = (Physics2D.CircleCast(node.parent.position, settings.collision.Evaluate(node.weight), math.normalize(node.position - node.parent.position), math.distance(node.position, node.parent.position), layerMask));
            if(hit.collider != null)
            {
                float2 pointOnLine = NearestPointOnLineSegment(node.parent.position, node.position, hit.point);
                float2 penetrationHeading = (float2)hit.point - pointOnLine;
                var penetrationShift = -math.normalize(penetrationHeading) * (settings.collision.Evaluate(node.weight) - math.length(penetrationHeading));

                //node.prevPosition += penetrationShift;
                node.position += penetrationShift;
                return true;
            }
            return false;
        }

        public void Update(float time)
        {
            float2 constantVelocity = new(settings.constantAcceleration.x, settings.constantAcceleration.y);
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
                    var convertFrom = n.parent.target.TransformPoint((Vector2)n.initLocalPosition);
                    float2 restPosition = new(convertFrom.x, convertFrom.y);
                    var displacement = restPosition - n.position;

                    //Hookes Law
                    //This expresses the elasticity constant in terms of newtons so we scale this by 100 for more manageable numbers
                    var elasticVelocity = displacement * elasticity * 100f;

                    var pos = n.position;
                    n.position = Maths.TimeCorrectedVerletIntegration2D(n.position, n.prevPosition, (1f - drag), constantVelocity + n.addedForce + elasticVelocity, (1f - damping), time, n.prevTimestep);
                    n.prevPosition = pos;
                    n.prevTimestep = time;
                    n.addedForce = float2.zero;

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
                        float angle = Quaternion.Angle(Quaternion.LookRotation((Vector2)(n.position - n.parent.position)), Quaternion.LookRotation(n.parent.target.TransformDirection((Vector2)n.initLocalPosition)));
                        if (angle > falloffAngle)
                        {
                            //Clamp to our angle limit
                            n.position = math.lerp(restPosition, n.position, falloffAngle / angle);
                        }
                    }

                    // Apply actualLength
                    var f2ParentPos = new float2(n.parent.target.position.x, n.parent.target.position.y);
                    n.position = f2ParentPos + math.normalize(n.position - f2ParentPos) * n.length;

                    if (collisionRadius > 0)
                        ResolveCollisionPenetration(n, layerMask);

                    n.target.localRotation = n.initLocalRotation;
                    var targetDirection = n.position - n.parent.position;
                    var rot = Quaternion.FromToRotation(n.parent.target.TransformDirection((Vector2)n.initLocalPosition), (Vector2)targetDirection);
                    n.target.rotation = rot * n.target.rotation;

                    // Apply the final position to the target
                    n.target.position = (Vector2)n.position;
                }
                else
                {
                    // Handle root node
                    n.position = new(n.target.position.x, n.target.position.y);
                    n.target.localRotation = n.initLocalRotation;
                    n.addedForce = float2.zero;
                }
            }
        }

        /// <summary>
        /// Adds a force to all nodes in the tree
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(float2 force)
        {
            foreach (var n in nodes)
                n.addedForce += force;
        }

        /// <summary>
        /// Adds a force to all nodes in the tree originating from the position provided and dampened by the length of the node over the node distance from the force position
        /// </summary>
        /// <param name="forceMagnitude">The strength of the force</param>
        /// <param name="position">The force's point of origin</param>
        public void AddForceAtPosition(float forceMagnitude, float2 position)
        {
            foreach (var n in nodes)
            {
                var d = math.distance(position, new(n.target.position.x, n.target.position.y));
                var l = n.parent != null ? math.distance(n.parent.target.position, n.target.position) : 0;
                if (l > 0 && d < l)
                {
                    float2 pPos = new(n.target.position.x, n.target.position.y);
                    float2 force = math.normalize(pPos - position) * (forceMagnitude * (d / l));
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
                    n.target.localPosition = (Vector2)n.initLocalPosition;
                    n.target.localRotation = n.initLocalRotation;
                    n.position = new(n.target.position.x, n.target.position.y);
                    n.prevPosition = new(n.target.position.x, n.target.position.y);
                    n.prevTimestep = Time.deltaTime;
                }
            }
        }

        private float2 NearestPointOnLineSegment(float2 lineStart, float2 lineEnd, float2 subject)
        {
            var line = (lineEnd - lineStart);
            var length = math.length(line);
            line = math.normalize(line);

            var subjectHeading = subject - lineStart;
            var dot = Vector2.Dot(subjectHeading, line);
            dot = Mathf.Clamp(dot, 0f, length);
            return lineStart + line * dot;
        }
    }
}

#endif