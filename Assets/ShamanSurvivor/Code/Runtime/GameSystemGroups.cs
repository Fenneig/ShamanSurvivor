using Unity.Entities;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class GameSimulationSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup), OrderFirst = true)]
    public partial class GameInputSystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameInputSystemGroup))]
    public partial class GameSpawnSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameInputSystemGroup))]
    public partial class GameMovementSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameMovementSystemGroup))]
    public partial class GameSpatialSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameSpatialSystemGroup))]
    public partial class GameSeparationSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameSeparationSystemGroup))]
    public partial class GameAbilitySystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(GameAbilitySystemGroup))]
    public partial class GameCombatSystemGroup : ComponentSystemGroup { }
}
