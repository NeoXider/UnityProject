#if HE_SYSCORE

using UnityEngine;
using System;
using Unity.Mathematics;

namespace HeathenEngineering.UnityPhysics
{
    [Serializable]
    public class VerletTransformNode2D
    {
        /// <summary>
        /// The transform this node refers to 
        /// </summary>
        public Transform target;
        [HideInInspector]
        public VerletTransformNode2D parent;
        [HideInInspector]
        public float distance;
        [HideInInspector]
        public float2 addedForce;
        [HideInInspector]
        public float weight;
        [HideInInspector]
        public float2 position;
        [HideInInspector]
        public float2 prevPosition;
        [HideInInspector]
        public float prevTimestep;
        [HideInInspector]
        public float2 initLocalPosition;
        [HideInInspector]
        public quaternion initLocalRotation;
        [HideInInspector]
        public float length;
    }
}

#endif
