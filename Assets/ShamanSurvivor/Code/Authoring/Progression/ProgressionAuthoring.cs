using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class ProgressionAuthoring : MonoBehaviour
    {
        [SerializeField] private ProgressionConfig _config;
        
        private class Baker : Baker<ProgressionAuthoring>
        {
            public override void Bake(ProgressionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new GameFlowState { Phase = GamePhase.Playing });
                AddComponent(entity, new LevelUpState { RandomState = authoring._config.RandomSeed, SelectedIndex = -1 });
                AddBuffer<LevelUpOption>(entity);
                DynamicBuffer<UpgradeDefinition> definitions = AddBuffer<UpgradeDefinition>(entity);

                foreach (var entry in authoring._config.Upgrades)
                {
                    definitions.Add(new UpgradeDefinition
                    {
                        Ability = entry.Ability,
                        Key = entry.Key,
                        BonusPerPick = entry.BonusPerPick,
                        MaxPicks = entry.MaxPicks
                    });
                }
            }
        }
    }
}