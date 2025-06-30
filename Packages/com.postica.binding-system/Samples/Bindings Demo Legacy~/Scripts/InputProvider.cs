using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class InputProvider : MonoBehaviour
    {
        public ReadOnlyBind<GameObject> cursor;
        public float forwardAmount = 1f;
        public float sidewaysAmount = 1f;
        public float minPointerDistance = 1f;
        public float maxPointerDistance = 10f;

        public float MoveForward { get; set; }
        public float MoveSideways { get; set; }
        public bool Fire { get; set; }
        public Vector3 TargetPosition { get; set; }

        private float _lastDistance = float.MaxValue;

        private void Start()
        {
            Cursor.visible = false;
        }

        void Update()
        {
            HandleForwardInput();
            HandleSidewaysInput();
            HandleTargetPosition();
            HandleFireInput();
        }

        private void HandleFireInput()
        {
            Fire = Input.GetMouseButton(0);
        }

        private void HandleTargetPosition()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Cursor.visible = true;
            }

            if (!Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            var distance = (maxPointerDistance - minPointerDistance);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ray.origin += ray.direction * minPointerDistance;


            if (Physics.Raycast(ray, out var hitInfo, distance, -1, QueryTriggerInteraction.Ignore))
            {
                TargetPosition = hitInfo.point;
                _lastDistance = hitInfo.distance;
            }
            else
            {
                TargetPosition = ray.origin + ray.direction * MathF.Min(_lastDistance, distance);
            }

            if (cursor.Value)
            {
                cursor.Value.transform.position = TargetPosition;
            }
        }

        private void HandleForwardInput()
        {
            if (Input.GetKey(KeyCode.W))
            {
                MoveForward = forwardAmount;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                MoveForward = -forwardAmount;
            }
            else
            {
                MoveForward = 0;
            }
        }

        private void HandleSidewaysInput()
        {
            if (Input.GetKey(KeyCode.D))
            {
                MoveSideways = sidewaysAmount;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                MoveSideways = -sidewaysAmount;
            }
            else
            {
                MoveSideways = 0;
            }
        }
    }

}