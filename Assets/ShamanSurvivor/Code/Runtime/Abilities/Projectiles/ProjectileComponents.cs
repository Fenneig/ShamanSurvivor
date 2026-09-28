using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Code.Runtime
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
}