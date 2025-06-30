#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.Demos
{

    public class SpawnObject : MonoBehaviour
    {
        public GameObject prefab;
        public Rigidbody target;
        public float projectileSpeed = 60;
        public LookAtInterceptPoint tracking;

        public void Spawn()
        {
            var go = Instantiate(prefab);
            var tran = go.GetComponent<Transform>();
            tran.position = tracking.selfTransform.position;
            tran.rotation = tracking.selfTransform.rotation;

            var ei = go.GetComponent<ExampleInterceptor>();
            if (ei != null)
            {
                ei.targetBody = target;
                ei.desiredSpeed = projectileSpeed;
            }
            else
            {
                var bp = go.GetComponent<BasicProjectileExample>();
                bp.speed = projectileSpeed;
            }
        }

        private void Update()
        {
            tracking.projectileSpeed = projectileSpeed;
        }
    }
}

#endif