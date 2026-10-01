using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [InternalBufferCapacity(8)]
    public struct UpgradeProgress : IBufferElementData
    {
        public AbilityId Ability;
        public UpgradeKey Key;

        public int Picks;

        public float TotalBonus;
    }

    [InternalBufferCapacity(16)]
    public struct UpgradeDefinition : IBufferElementData
    {
        public AbilityId Ability;
        public UpgradeKey Key;

        public float BonusPerPick;

        public int MaxPicks;
    }
}