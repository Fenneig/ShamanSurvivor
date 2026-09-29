using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public enum GamePhase : byte
    {
        Playing = 0,
        LevelUp = 1
    }

    public struct GameFlowState : IComponentData
    {
        public GamePhase Phase;
    }
    
    public struct LevelUpState : IComponentData
    {
        public int SelectedIndex;
        public uint RandomState;
    }

    [InternalBufferCapacity(3)]
    public struct LevelUpOption : IBufferElementData
    {
        public AbilityId Ability;
        public UpgradeKey Key;

        public float Bonus;
        
        public int CurrentPicks;
        public int MaxPicks;
    }
}