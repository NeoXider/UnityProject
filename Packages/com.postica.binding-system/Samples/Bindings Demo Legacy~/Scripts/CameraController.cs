using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class CameraController : MonoBehaviour
    {
        public Transform targetToFollow;
        [Bind]
        [Range(0f, 1f)]
        public ReadOnlyBind<float> followStrength = 0.5f.Bind();

        [Bind]
        [Min(0)]
        public ReadOnlyBind<float> deadAngle = 30f.Bind();


        private Transform preciseFollower;

        private void Start()
        {
            preciseFollower = new GameObject("PreciseFollower").transform;
            preciseFollower.SetPositionAndRotation(transform.position, transform.rotation);
            preciseFollower.SetParent(targetToFollow, true);
        }

        void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, preciseFollower.position, followStrength);
            transform.rotation = Quaternion.Slerp(transform.rotation, preciseFollower.rotation, followStrength);
        }
    } 
}
