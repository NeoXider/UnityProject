#if HE_SYSCORE

using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CreateAssetMenu(menuName = "Physics/Effects/Wind")]
    public class WindEffect : ForceEffect
    {
        public bool EffectTorque = false;
        public bool EffectLinear = false;
        public float volumetricDensity = 1.239f;
        public float metersPerSecond = 1f;
        public float frequency = 10f;
        public float magnitude = 1f;
        public float turbulence = 0.25f;

        public override void AngularEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            
            if (EffectTorque)
            {
                var dir = origin - subjectData.SelfTransform.position;

                if (dir != Vector3.zero)
                {
                    //Rough simulation of angular drag
                    var windForce = API.Maths.SimulateWindForce(metersPerSecond, frequency, magnitude, turbulence, dir, Time.time);
                    windForce = API.Maths.QuadraticDrag(subjectData.AngularDragCoefficient, volumetricDensity, windForce, subjectData.CrossSection(math.normalize(windForce)));

                    subjectData.AttachedRigidbody.AddTorque((API.Maths.TorqueToReachDirection(subjectData.AttachedRigidbody, math.normalize(windForce)) / subjectData.Mass) * (volumetricDensity / subjectData.Density) * strength, ForceMode.Force);
                }
            }
        }

        public override void LinearEffect(Vector3 origin, float strength, PhysicsData subjectData)
        {
            if (EffectLinear)
            {
                var dir = origin - subjectData.SelfTransform.position;
                if (dir != Vector3.zero)
                {
                    var windForce = API.Maths.SimulateWindForce(metersPerSecond, frequency, magnitude, turbulence, dir, Time.time);

                    windForce = API.Maths.QuadraticDrag(subjectData.LinearDragCoefficient, volumetricDensity, windForce, subjectData.CrossSection(math.normalize(windForce)));
                    subjectData.AttachedRigidbody.AddForce(windForce * (volumetricDensity / subjectData.Density) * strength, ForceMode.Force);
                }
            }
        }
    }
}

#endif
