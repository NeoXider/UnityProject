using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public interface IProjectileSpawner
    {
        IProjectile Spawn(Vector3 position);
    }

    public interface IProjectile
    {
        delegate void OnCollisionDelegate(IProjectile projectile, Collision collision);

        Vector3 Velocity { get; set; }
        float Mass { get; set; }
        float Lifetime { get; set; }

        event OnCollisionDelegate Collided;
    }
}