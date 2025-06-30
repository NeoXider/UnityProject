#if HE_SYSCORE

using UnityEngine;
using System;
using Unity.Mathematics;

namespace HeathenEngineering.UnityPhysics
{
    [Serializable]
    public class VerletTransformNode
    {
        /// <summary>
        /// The transform this node refers to 
        /// </summary>
        public Transform target;
        [HideInInspector]
        public VerletTransformNode parent;
        [HideInInspector]
        public float distance;
        [HideInInspector]
        public float3 addedForce;
        [HideInInspector]
        public float weight;
        [HideInInspector]
        public float3 position;
        [HideInInspector]
        public float3 prevPosition;
        [HideInInspector]
        public float prevTimestep;
        [HideInInspector]
        public float3 initLocalPosition;
        [HideInInspector]
        public quaternion initLocalRotation;
        [HideInInspector]
        public float length;
    }
}

#endif
