using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.UnityPhysics
{
    public class Breakable : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody parentRigidbody;
        public GameObject unbroken;
        public GameObject broken;

        [Header("General Settings")]
        [Tooltip("If true the Collision Break event will trigger on a sufficent collision force but break function will not be called.\nThis can be used to sync with various networking APIs.")]
        public bool eventOnly = false;
        [Tooltip("The time after break before the object is destroyed.\nTo disable this effect set the value to 0.")]
        public float destroyTimer = 0;
        [Tooltip("The time after break before the child rigidbodies are disabled.\nTo disable this effect set the value to 0.")]
        public float disableTimer = 0;

        [Header("Collision Settings")]
        [Tooltip("The force required to break the object.\nTo disable this effect set the value to 0.")]
        public float breakForce = 0;
        [Tooltip("The force added to the point of collision when breaking due to break force.\nTo disable this effect set the value to 0.")]
        public float addForce = 0;
        [Tooltip("The radius of the force added to the point of collision when breaking due to break force.\nTo disable this effect set the value to 0.")]
        public float addForceRadius = 0;

        [Header("Events")]
        public UnityEvent collisionBreak;
        public UnityEvent breaking;
        public UnityEvent physicsDisabled;
        public UnityEvent destroying;

        public bool IsBroken { get; set; }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsBroken && breakForce > 0)
            {
                if (collision.relativeVelocity.magnitude > breakForce)
                {
                    if (!eventOnly)
                    {
                        Break();
                        if (addForce > 0)
                        {
                            var impactPoint = collision.GetContact(0).point;
                            foreach (var body in GetComponentsInChildren<Rigidbody>(true))
                            {
                                body.AddExplosionForce(addForce, impactPoint, addForceRadius);
                            }
                        }
                    }
                    collisionBreak.Invoke();
                }
            }
        }

        [ContextMenu("Break")]
        public void Break()
        {
            if (IsBroken)
                return;
            else
                IsBroken = true;

            if (parentRigidbody != null)
                parentRigidbody.isKinematic = true;

            unbroken.SetActive(false);
            broken.SetActive(true);

            if (destroyTimer > 0)
                Invoke("DestroyObject", destroyTimer);

            if (disableTimer > 0)
                Invoke("DisablePhysics", disableTimer);

            breaking.Invoke();
        }

        private void DestroyObject()
        {
            destroying.Invoke();

            var destroyEffect = GetComponent<IDestroyEffect>();
            if (destroyEffect != null)
                destroyEffect.Destroy();
            else
                Destroy(gameObject);
        }

        private void DisablePhysics()
        {
            foreach (var body in GetComponentsInChildren<Rigidbody>())
                body.isKinematic = true;
            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            physicsDisabled.Invoke();
        }
    }
}
