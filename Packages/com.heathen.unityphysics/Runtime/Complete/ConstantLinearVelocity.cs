#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class ConstantLinearVelocity : MonoBehaviour
    {
        public Vector3Reference targetRelativeVelocity;

        private Rigidbody selfBody = null;

        private void Start()
        {
            selfBody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            selfBody.AddForce(API.Maths.ForceToReachLinearVelocity(selfBody, targetRelativeVelocity.Value), ForceMode.Force);
        }
    }
}


#endif