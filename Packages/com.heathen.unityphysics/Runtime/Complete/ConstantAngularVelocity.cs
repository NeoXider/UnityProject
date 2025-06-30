#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class ConstantAngularVelocity : MonoBehaviour
    {
        public Vector3Reference targetRelativeVelocity;

        private Rigidbody selfBody = null;

        private void Start()
        {
            selfBody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            selfBody.angularVelocity = targetRelativeVelocity.Value;
        }
    }
}


#endif