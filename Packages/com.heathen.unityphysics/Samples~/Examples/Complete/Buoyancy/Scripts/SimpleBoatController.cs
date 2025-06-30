#if HE_SYSCORE

using HeathenEngineering.UnityPhysics;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class SimpleBoatController : MonoBehaviour
    {
        public Transform selfTransform;
        public Rigidbody selfBody;
        public float thrust;

        private BuoyantBody bBody;

        private void Start()
        {
            selfTransform = GetComponent<Transform>();
            selfBody = GetComponent<Rigidbody>();
            bBody = GetComponent<BuoyantBody>();
        }

        private void FixedUpdate()
        {
            if (bBody.IsFloating)
            {
                Vector3 direction = new Vector3();

                if (Input.GetKey(KeyCode.W))
                {
                    direction += selfTransform.forward;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    direction -= selfTransform.forward;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    direction -= selfTransform.right * 0.5f;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    direction += selfTransform.right * 0.5f;
                }

                if (direction != Vector3.zero)
                    selfBody.AddForceAtPosition(direction * thrust, (selfTransform.position - (selfTransform.forward * 2f)) - (selfTransform.up * 0.1f));
            }
        }
    }
}

#endif