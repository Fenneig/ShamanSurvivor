using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct ChainLightningDefinition : IComponentData
    {
        public AbilityId Ability;
        public float JumpRange;
        public int BaseJumps;
    }
}