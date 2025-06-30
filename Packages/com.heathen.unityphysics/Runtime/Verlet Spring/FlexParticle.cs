#if HE_SYSCORE

using UnityEngine;
using Unity.Mathematics;
using HeathenEngineering.UnityPhysics.API;
using System;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Experimental
    /// </summary>
    [Obsolete("Experimental particle tool, will change or be removed in the future")]
    public class FlexParticle : MonoBehaviour
    {
        // Parameters for influences
        [System.Serializable]
        public class Influence
        {
            public Transform subject;
            public float scalar;
            public float distance;
            [HideInInspector]
            /// <summary>
            /// distance from subject to self
            /// </summary>
            public float length;
            public float heading;
            [HideInInspector]
            /// <summary>
            /// Heading from the subject to self
            /// </summary>
            public float3 headingDirection;
            public float facing;
            [HideInInspector]
            /// <summary>
            /// rotation from the subject's rotation such that if this rotation was applied to the subjects
            /// rotation the object would be relatively facing the same way relative to the subject 
            /// </summary>
            public quaternion facingRotation;
        }

        public Influence[] influences;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            foreach (Influence influence in influences)
            {
                influence.length = math.distance(transform.position, influence.subject.position);
                influence.headingDirection = math.normalize(transform.position - influence.subject.position);
                influence.facingRotation = Maths.QuaternionDifference(influence.subject.rotation, transform.rotation);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float3 totalForce = float3.zero;
            Vector3 totalTorque = Vector3.zero;

            // Calculate forces from influences
            foreach (Influence influence in influences)
            {
                if (influence.subject != null)
                {
                    if (influence.distance > 0)
                    {
                        //calculate the force applied to keep length using distance as a scaler for the strength of this force
                        float3 directionToSubject = transform.position - influence.subject.position;
                        float3 nDirection = math.normalize(directionToSubject);
                        float tDistance = math.length(directionToSubject);

                        float change = tDistance - influence.length;

                        float3 force = -nDirection * change * influence.distance;
                        totalForce += force * influence.scalar;
                    }
                    if (influence.heading > 0)
                    {
                        //calculate the force applied to keep relative direction from subject using heading as a scalar for the strength of this force
                        float3 desiredDirection = Quaternion.Euler(0f, influence.heading, 0f) * influence.headingDirection;
                        float3 directionToDesired = desiredDirection - influence.headingDirection;
                        float3 force = math.normalize(directionToDesired) * influence.heading;
                        totalForce += force * influence.scalar;
                        //calculate torque impact based on heading change
                        float3 currentForward = transform.forward;
                        Quaternion rotationChange = Quaternion.FromToRotation(currentForward, desiredDirection);
                        Vector3 torque = rotationChange.eulerAngles * influence.heading;
                        totalTorque += torque * influence.scalar;
                    }
                    if (influence.facing > 0)
                    {
                        //calculate the torque applied to keep relative rotation with the subject using facing as a scalar for the strength of this force
                        Quaternion targetRotation = influence.subject.rotation * influence.facingRotation;
                        Quaternion torqueRotation = Maths.QuaternionDifference(transform.rotation, targetRotation);
                        totalTorque += torqueRotation.eulerAngles * influence.facing * influence.scalar;
                    }
                }
            }

            if (rb != null)
            {
                rb.AddForce(totalForce, ForceMode.Acceleration);
                rb.AddTorque(totalTorque, ForceMode.Acceleration);
            }
            else
            {
                transform.position += (Vector3)(totalForce);
                transform.rotation *= Quaternion.Euler(totalTorque);
            }
        }
    }
}

#endif
