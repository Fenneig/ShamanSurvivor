using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct ProjectileTag : IComponentData{}

    public struct ProjectileBody : IComponentData
    {
        public float HitRadius;
    }
    
    public struct Projectile : IComponentData
    {
        public Entity Source;

        public float2 Direction;

        public float Speed;
        public float Damage;

        public float RemainingLifetime;
        
        public DamageElement Element;
    }

    public struct ProjectileModifierSnapshot : IComponentData
    {
        public float SizeMultiplier;
        public float DurationMultiplier;
        public float EffectStrengthMultiplier;
        public float DamageMultiplier;
    }

    public struct ProjectileHitEvent : IComponentData
    {
        public Entity Target;
        
        public float2 Position;
    }
    
    public struct DirectImpact : IComponentData { }
    
    public struct PiercingProjectile : IComponentData{ }

    [InternalBufferCapacity(8)]
    public struct ProjectileHitHistory : IBufferElementData
    {
        public Entity Entity;
    }
}