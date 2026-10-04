using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct AbilityDefinition : IBufferElementData
    {
        public AbilityId Ability;

        public DamageElement DamageElement;

        public float Damage;
        public float Range;
        public float AttackInterval;

        public bool CanUnlock;
    }
}