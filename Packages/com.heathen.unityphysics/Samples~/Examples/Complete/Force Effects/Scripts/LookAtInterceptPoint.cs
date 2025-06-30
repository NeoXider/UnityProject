#if HE_SYSCORE

using HeathenEngineering.UnityPhysics.API;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class LookAtInterceptPoint : MonoBehaviour
    {
        public Transform selfTransform;
        public float projectileSpeed;
        public Rigidbody target;

        private void FixedUpdate()
        {
            var point = Maths.FastIntercept(selfTransform.position, projectileSpeed, target);
            selfTransform.LookAt(point);
        }
    }
}

#endif