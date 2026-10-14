using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct LightningShieldDefinition : IComponentData
    {
        public float OrbitRadius;
        public float OrbHitRadius;
        public int BaseOrbCount;
    }

    public struct LightningShieldOrbitState : IComponentData
    {
        public double TotalAngle;

        public float2 PreviousPlayerPosition;

        public int PreviousCount;
        public float PreviousRadius;

        public bool Initialized;
    }

    public struct LightningOrbReference : IBufferElementData
    {
        public Entity Value;
    }

    public struct LightningOrbActive : IComponentData, IEnableableComponent
    {
    }

    public struct LightningOrb : IComponentData
    {
        public Entity Owner;

        public int Slot;
        public int OrbCount;

        public float2 FromCenter;
        public float2 ToCenter;

        public double FromAngle;
        public double ToAngle;

        public float OrbitRadius;
        public float HitRadius;

        public float Damage;
        public DamageElement Element;
    }

    [InternalBufferCapacity(8)]
    public struct LightningOrbHit : IBufferElementData
    {
        public Entity Target;

        public double LastHitAngle;
    }
}