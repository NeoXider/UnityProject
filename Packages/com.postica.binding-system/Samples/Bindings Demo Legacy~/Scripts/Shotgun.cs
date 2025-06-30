using UnityEngine;

namespace Postica.BindingSystem.Samples
{

    public class Shotgun : MonoBehaviour, IWeapon
    {
        [Header("Components")]
        public Transform muzzle;
        public ReadOnlyBind<AudioSource> audioSource;
        public BindDataFor<IProjectileSpawner> projectilePrefab;

        [Header("Settings")]
        public ReadOnlyBind<int> projectilesToFire = 3.Bind();
        public ReadOnlyBind<float> projectileSpeed = 10f.Bind();
        public ReadOnlyBind<float> projectileSpread = 0.2f.Bind();
        public ReadOnlyBind<float> projectileMass = 0.2f.Bind();
        public ReadOnlyBind<float> projectileLifetime = 5f.Bind();
        public ReadOnlyBind<AudioClip> fireSound;

        [Header("Output")]
        [WriteOnlyBind]
        public Bind<float> hitDamage;

        private float _accumulatedDamage;

        public void Fire()
        {
            for (int i = 0; i < projectilesToFire; i++)
            {
                var spreadX = Random.Range(-projectileSpread, projectileSpread);
                var spreadY = Random.Range(-projectileSpread, projectileSpread);
                var projectile = projectilePrefab.Value.Spawn(muzzle.position);
                projectile.Lifetime = projectileLifetime;
                projectile.Mass = projectileMass;
                projectile.Velocity = (muzzle.forward + muzzle.right * spreadX + muzzle.up * spreadY).normalized * projectileSpeed ;
                projectile.Collided += Projectile_Collided;
            }

            if (audioSource.Value != null && fireSound.Value != null)
            {
                audioSource.Value.clip = fireSound;
                audioSource.Value.Play();
            }
        }

        private void Projectile_Collided(IProjectile projectile, Collision collision)
        {
            _accumulatedDamage += collision.impulse.magnitude;
        }

        private void Update()
        {
            hitDamage.Value = _accumulatedDamage;
            _accumulatedDamage = 0;
        }
    }
}