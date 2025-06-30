#if HE_SYSCORE

using UnityEngine;
using System;
using Unity.Mathematics;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Variables/Verlet Transform Tree Settings")]
    [Serializable]
    public class VerletTransformTreeSettings : ScriptableObject
    {
        [Header("Properties")]
        public float3 constantAcceleration = new Vector3(0, -9.81f, 0);
        public ScaledAnimationCurve drag = ScaledAnimationCurve.EaseInOut(0.01f, 0f, 2f, 1, 3.5f);
        public ScaledAnimationCurve damping = ScaledAnimationCurve.EaseInOut(0.02f, 0f, 1f, 1, 1f);
        public ScaledAnimationCurve elasticity = ScaledAnimationCurve.EaseInOut(1f, 0f, 1f, 1f, 1f);
        public ScaledAnimationCurve stiffness = ScaledAnimationCurve.EaseInOut(0.01f,  0f, 1f, 1, 1f);
        
        [Header("Limiters")]
        public LayerMask collisionLayers = 0;
        public ScaledAnimationCurve collision = ScaledAnimationCurve.EaseInOut(0f, 0f, 0f, 1f, 0f);
        public ScaledAnimationCurve angle = ScaledAnimationCurve.EaseInOut(0f, 0f, 0f, 1f, 0f);
    }
}

#endif