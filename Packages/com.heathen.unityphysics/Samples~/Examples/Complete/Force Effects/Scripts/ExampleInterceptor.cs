#if HE_SYSCORE

using HeathenEngineering.UnityPhysics.API;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class ExampleInterceptor : MonoBehaviour
    {
        public bool destroyOnIntercept = true;
        [Tooltip("Once target distance is within this range and increase in distance will be treated as a fly by and will cause a self destruct")]
        public float proximityTest = 5;
        public float desiredSpeed = 3;
        public float actualSpeed = 0;
        public Rigidbody selfBody;
        public Rigidbody targetBody;

        private Vector3 targetPoint;
        private float priorDistance = float.MaxValue;

        // Update is called once per frame
        void FixedUpdate()
        {
            var dis = Vector3.Distance(selfBody.position, targetBody.position);

            if (destroyOnIntercept)
            {
                if (dis < 1f || (priorDistance < proximityTest && dis > priorDistance))
                {
                    Destroy(gameObject);
                    return;
                }
            }

            priorDistance = dis;

            /**************************************************************************
             * Demonstration of HeathenMath formula
             **************************************************************************/

            //Find the force required to accelerate to the desired speed
            var acceleration = Maths.ForceToReachLinearVelocity(selfBody, selfBody.transform.forward * desiredSpeed);
            //Apply the force
            selfBody.AddForce(acceleration, ForceMode.Force);

            //Get the actual speed of the body; mostly for display purposes but useful else where as well
            actualSpeed = selfBody.velocity.magnitude;

            //Find the point of intercept with the target
            targetPoint = Maths.FastIntercept(selfBody.position, actualSpeed, targetBody);
            //Find the direction to the point of intercept
            var directionToPoint = (targetPoint - selfBody.position).normalized;
            //Find the torque required to rotate to the direction to point/target
            var torqueForce = Maths.TorqueToReachDirection(selfBody, directionToPoint);
            //Apply the torque
            selfBody.AddTorque(torqueForce, ForceMode.Impulse);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(selfBody.position, targetPoint);
        }
    }
}

#endif