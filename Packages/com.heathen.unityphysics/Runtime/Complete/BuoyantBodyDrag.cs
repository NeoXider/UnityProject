#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(PhysicsData))]
    [RequireComponent(typeof(BuoyantBody))]
    public class BuoyantBodyDrag : MonoBehaviour
    {
        public float surfaceLinearDrag = 0.1f;
        public float subsurfaceLinearDrag = 3f;

        public float surfaceAngularDrag = 0.1f;
        public float subsurfaceAngularDrag = 3f;

        public void UpdateDrag(BuoyantBody body, PhysicsData physicsData)
        {
            if (body != null && physicsData != null)
            {
                physicsData.AttachedRigidbody.drag = (subsurfaceLinearDrag * body.submergedRatio) + ((1f - body.submergedRatio) * surfaceLinearDrag);
                physicsData.AttachedRigidbody.angularDrag = (subsurfaceAngularDrag * body.submergedRatio) + ((1f - body.submergedRatio) * surfaceAngularDrag);
            }
        }
    }
}


#endif