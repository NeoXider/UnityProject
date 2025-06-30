using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class SinkingDestroy : MonoBehaviour, IDestroyEffect
    {
        public Vector3 direction = new Vector3(0, -1, 0);
        public float speed = 0.25f;
        public float duration = 5f;

        public bool IsDestroying { get; private set; }
        private Transform selfTransform;

        public void Destroy()
        {
            IsDestroying = true;
            selfTransform = GetComponent<Transform>();
            Invoke("DestroyObject", duration);
        }

        private void Update()
        {
            if (IsDestroying)
            {
                selfTransform.position += direction * speed * Time.deltaTime;
            }
        }

        private void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}
