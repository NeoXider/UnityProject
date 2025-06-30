#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class BasicProjectileExample : MonoBehaviour
    {
        public float speed = 120;

        Rigidbody selfBody;

        private void Start()
        {
            selfBody = GetComponent<Rigidbody>();
            Invoke("TimeKill", 5f);
            selfBody.drag = 0;
            selfBody.angularDrag = 0;
            selfBody.velocity = transform.forward * speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }

        void TimeKill()
        {
            Destroy(gameObject);
        }
    }
}

#endif