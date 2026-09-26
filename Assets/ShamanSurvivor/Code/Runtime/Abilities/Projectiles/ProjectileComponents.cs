using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    public struct ProjectileTag : IComponentData{}

    public struct ProjectileBody : IComponentData
    {
        public float HitRadius;
    }
    
    public struct HomingProjectile : IComponentData
    {
        public Entity Source;
        public Entity Target;

        public float Speed;
        public float Damage;

        public float RemainingLifetime;
        
        public DamageElement DamageElement;
    }
}