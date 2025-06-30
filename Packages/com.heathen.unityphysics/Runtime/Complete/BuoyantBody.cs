#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(PhysicsData))]
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyantBody : HeathenBehaviour
    {
        public Vector3Reference buoyantMagnitude = new Vector3Reference(new Vector3(0, -9.8f * 2f, 0));
        private PhysicsData physicsData;
        public SurfaceTool activeSurface;
        
        public CalculationMode calculationMode = CalculationMode.Fast;
        public bool IsFloating => submergedRatio > 0;
        [HideInInspector]
        public float submergedRatio = 0f;
        private float buoyantForce = 0;
        private BuoyantBodyDrag dragHelper;

        private void Start()
        {
            physicsData = GetComponent<PhysicsData>();
            dragHelper = GetComponent<BuoyantBodyDrag>();
        }

        private void FixedUpdate()
        {
            if(physicsData != null && activeSurface != null)
            {
                Vector3 grav = buoyantMagnitude.Value;

                if (calculationMode == CalculationMode.Fast)
                {
                    buoyantForce = API.Buoyancy.Effect(physicsData, activeSurface, grav.magnitude, out float volume) * Time.fixedDeltaTime;

                    submergedRatio = Mathf.Clamp01(volume / physicsData.volume);

                    if (dragHelper != null)
                        dragHelper.UpdateDrag(this, physicsData);

                    physicsData.AttachedRigidbody.AddForce(buoyantForce * -grav.normalized, ForceMode.Force);
                }
                else if (calculationMode == CalculationMode.Simple)
                {
                    buoyantForce = API.Buoyancy.Effect(physicsData, activeSurface, grav.magnitude, out float volume) * Time.fixedDeltaTime;

                    submergedRatio = Mathf.Clamp01(volume / physicsData.volume);

                    if (dragHelper != null)
                        dragHelper.UpdateDrag(this, physicsData);

                    physicsData.AttachedRigidbody.AddForce(buoyantForce * -grav.normalized, ForceMode.Force);
                    var normal = activeSurface.SurfaceNormal(physicsData.SelfTransform.position);
                    physicsData.AttachedRigidbody.AddTorque(Vector3.Cross(physicsData.SelfTransform.up, normal) * 2, ForceMode.VelocityChange);
                }
                else
                {
                    buoyantForce = API.Buoyancy.Effect(physicsData, activeSurface, grav.magnitude, out List<PointVolume> applicationPoints, out float volume) * Time.fixedDeltaTime;

                    submergedRatio = Mathf.Clamp01(volume / physicsData.volume);

                    if (dragHelper != null)
                        dragHelper.UpdateDrag(this, physicsData);

                    foreach (var p in applicationPoints)
                    {
                        physicsData.AttachedRigidbody.AddForceAtPosition(buoyantForce * (p.volume / physicsData.volume) * -grav.normalized, physicsData.SelfTransform.position +  physicsData.SelfTransform.TransformVector(p.point), ForceMode.Force);
                    }
                }
            }
        }
    }
}

#endif