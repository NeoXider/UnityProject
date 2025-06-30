#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(TrickShot))]
    public class TrickShotConstantAcceleration : MonoBehaviour
    {
        public List<Vector3> globalConstants = new(new Vector3[] { new(0, -9.81f, 0) });
        public List<Vector3> localConstants = new();

        private TrickShot ts;

        private void Start()
        {
            ts = GetComponent<TrickShot>();
        }

        private void LateUpdate()
        {
            Vector3 sum = new Vector3();
            foreach(var v in globalConstants)
                sum += v;
            foreach(var v in localConstants)
                sum += ts.transform.rotation * v;

            ts.constantAcceleration = sum;
        }
    }
}

#endif