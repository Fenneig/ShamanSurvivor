using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [InternalBufferCapacity(8)]
    public struct PassiveProgress : IBufferElementData
    {
        public GlobalPassiveId Passive;
        public int Picks;
        public float TotalBonus;
    }

    [InternalBufferCapacity(8)]
    public struct PassiveDefinition : IBufferElementData
    {
        public GlobalPassiveId Passive;
        public float BonusPerPick;
        public int MaxPicks;
    }
}