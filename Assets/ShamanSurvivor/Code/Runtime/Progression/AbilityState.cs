using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct AbilityState : IBufferElementData
    {
        public AbilityId Ability;

        public float CooldownRemaining;
    }
}