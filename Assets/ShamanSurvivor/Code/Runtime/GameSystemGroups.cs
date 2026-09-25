using Unity.Entities;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    /// <summary>
    /// Основная игровая симуляция.
    /// Выполняется до пересчёта ECS transforms.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class GameSimulationSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSimulationSystemGroup), OrderFirst = true)]
    public partial class GameInputSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSimulationSystemGroup))]
    [UpdateAfter(typeof(GameInputSystemGroup))]
    public partial class GameMovementSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(GameMovementSystemGroup))]
    public partial class GameCombatSystemGroup : ComponentSystemGroup
    {
    }
}
