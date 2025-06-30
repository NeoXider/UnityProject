using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class CollisionMousePointer : MonoBehaviour
    {
        public enum MouseButton
        {
            LeftClick = 0,
            RightClick = 1,
            MiddleClick = 2
        }
        
        [Header("Setup")]
        public ReadOnlyBind<Camera> sourceCamera;
        public ReadOnlyBind<LayerMask> collisionLayer = ((LayerMask)(-1)).Bind();
        public ReadOnlyBind<MouseButton> mouseButton = MouseButton.LeftClick.Bind();
        public ReadOnlyBind<bool> clickOnlyOnRaycastHit = true.Bind();

        [Header("Output")] 
        public Bind<bool> click;
        public Bind<float> clickDuration;
        public Bind<Vector3> pointerPosition;

        private float _clickDuration;
        
        public bool IsClicked { get; private set; }
        
        void Update()
        {
            var cam = sourceCamera.Value;
            if (!cam)
            {
                cam = Camera.main;
            }
            
            // Get the point where the mouse is pointing
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, collisionLayer.Value, QueryTriggerInteraction.Ignore))
            {
                pointerPosition.Value = hit.point;
            }
            else if(clickOnlyOnRaycastHit)
            {
                return;
            }
            
            var thisFrameClick = Input.GetMouseButton((int) mouseButton.Value);
            if (thisFrameClick)
            {
                _clickDuration += Time.deltaTime;
                clickDuration.Value = _clickDuration;
                click.Value = IsClicked = true;
            }
            else
            {
                _clickDuration = 0;
                clickDuration.Value = 0;
                click.Value = IsClicked = false;
            }
        }
    } 
}
