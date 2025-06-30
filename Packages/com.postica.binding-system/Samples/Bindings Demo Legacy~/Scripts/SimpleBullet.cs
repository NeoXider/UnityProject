using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleBullet : MonoBehaviour, IProjectile, IProjectileSpawner
    {
        [Min(0.1f)]
        public float lifetime = 10;

        private Rigidbody body;
        private int readyFrame;

#if UNITY_6000_0_OR_NEWER
        public Vector3 Velocity { get => body.linearVelocity; set => body.linearVelocity = value; }
#else
        public Vector3 Velocity { get => body.velocity; set => body.velocity = value; }
#endif
        public float Mass { get => body.mass; set => body.mass = value; }
        public float Lifetime
        {
            get => lifetime; 
            set => lifetime = value;
        }

        public event IProjectile.OnCollisionDelegate Collided;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
            readyFrame = Time.frameCount + 2;
        }

        public IProjectile Spawn(Vector3 position)
        {
            var clone = Instantiate(this);
            clone.transform.position = position;
            clone.gameObject.SetActive(true);
            return clone;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(Time.frameCount < readyFrame)
            {
                return;
            }

            Collided?.Invoke(this, collision);
        }
    }
}