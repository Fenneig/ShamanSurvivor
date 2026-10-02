using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class ProgressionAuthoring : MonoBehaviour
    {
        [SerializeField] private LevelUpCardCatalog _config;
        [SerializeField] private uint _randomSeed;
        
        private class Baker : Baker<ProgressionAuthoring>
        {
            public override void Bake(ProgressionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new GameFlowState { Phase = GamePhase.Playing });
                AddComponent(entity, new LevelUpState 
                {
                    RandomState = authoring._randomSeed, 
                    SelectedIndex = -1,
                    Revision = 0
                });
                AddBuffer<LevelUpOption>(entity);
                
                DynamicBuffer<UpgradeDefinition> definitions = AddBuffer<UpgradeDefinition>(entity);

                foreach (var abilityEntry in authoring._config.Abilities)
                {
                    foreach (var keyEntry in abilityEntry.Keys)
                    {
                        definitions.Add(new UpgradeDefinition
                        {
                            Ability = abilityEntry.Ability,
                            Key = keyEntry.Key,
                            BonusPerPick = keyEntry.BonusPerPick,
                            MaxPicks = keyEntry.MaxPicks
                        });
                    }
                }
                
                DynamicBuffer<PassiveDefinition> passiveDefinitions = AddBuffer<PassiveDefinition>(entity);

                foreach (var passive in authoring._config.Passives)
                {
                    passiveDefinitions.Add(new PassiveDefinition
                    {
                        Passive = passive.Passive,
                        BonusPerPick = passive.BonusPerPick,
                        MaxPicks = passive.MaxPicks
                    });
                }
            }
        }
    }
}