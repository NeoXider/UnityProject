#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        public ReadOnlyBind<IWeapon> weapon;

        [Header("Settings")]
        [Bind] // Use Bind attribute above other attributes to apply the attributes to inner value
#if ODIN_INSPECTOR
        [SuppressInvalidAttributeError]
#endif
        [Range(0, 20)]
        public ReadOnlyBind<float> maxSpeed = 5f.Bind();
        [Bind]
        [Range(0, 120)]
#if ODIN_INSPECTOR
        [SuppressInvalidAttributeError]
#endif
        public ReadOnlyBind<float> maxRotationSpeed = 5f.Bind();
        [Bind]
        [Range(0, 90)]
#if ODIN_INSPECTOR
        [SuppressInvalidAttributeError]
#endif
        public ReadOnlyBind<float> minRotationAngle = 45f.Bind();

        [Bind]
        [Range(0.01f, 10f)]
#if ODIN_INSPECTOR
        [SuppressInvalidAttributeError]
#endif
        public ReadOnlyBind<float> fireFrequency = 2f.Bind();

        [Space]
        public ReadOnlyBind<bool> preferForce = true.Bind();
        public ReadOnlyBind<ForceMode> forceMode = ForceMode.Acceleration.Bind();

        [Header("Input")]
        public ReadOnlyBind<float> forwardSpeed;
        public ReadOnlyBind<float> sidewaysSpeed;

        public ReadOnlyBind<Vector3> targetPointer;
        public ReadOnlyBind<bool> fire;

        private Rigidbody body;

        private float nextFireAvailable;

        // Start is called before the first frame update
        void Start()
        {
            body = GetComponentInChildren<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            MoveCharacter();
            RotateCharacter();
            RotateWeapon();
            CheckForFireCommand();
        }

        private void RotateWeapon()
        {
            if(weapon.Value is not Component weaponComponent)
            {
                return;
            }
            var weaponTransform = weaponComponent.transform;
            weaponTransform.LookAt(targetPointer);
        }

        private void CheckForFireCommand()
        {
            if (!fire || !(nextFireAvailable < Time.time)) return;
            
            weapon.Value.Fire();
            nextFireAvailable = Time.time + 1 / fireFrequency;
        }

        private void RotateCharacter()
        {
            var screenTargetPosition = Camera.main.WorldToScreenPoint(targetPointer);
            var ray = Camera.main.ScreenPointToRay(screenTargetPosition);
            var newForward = ray.GetPoint(10);
            var deltaRotation = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(newForward.normalized, transform.up));
            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if(Mathf.Abs(angle) < minRotationAngle)
            {
                return;
            }

            if(Vector3.Dot(axis, transform.up) < 0)
            {
                angle = -angle;
            }
            if (preferForce)
            {
                body.AddRelativeTorque(Vector3.up * Mathf.Clamp(Mathf.Deg2Rad * angle, -maxRotationSpeed, maxRotationSpeed), forceMode);
            }
            else
            {
                var clampedDeltaRotation = Quaternion.Euler(axis * Mathf.Clamp(angle, -maxRotationSpeed, maxRotationSpeed));
                body.MoveRotation(body.rotation * clampedDeltaRotation);
            }
        }

        private void MoveCharacter()
        {
            if (preferForce)
            {
                body.AddRelativeForce(Vector3.forward * Mathf.Clamp(forwardSpeed, -maxSpeed, maxSpeed), forceMode);
                body.AddRelativeForce(Vector3.right * Mathf.Clamp(sidewaysSpeed, -maxSpeed, maxSpeed), forceMode);
            }
            else
            {
                var forward = body.transform.forward * Mathf.Clamp(forwardSpeed, -maxSpeed, maxSpeed);
                var sideways = body.transform.right * Mathf.Clamp(sidewaysSpeed, -maxSpeed, maxSpeed);
                body.MovePosition(body.position + forward + sideways);
            }
        }
    }

}