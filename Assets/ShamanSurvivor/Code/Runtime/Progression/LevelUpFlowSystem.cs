using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using ShamanSurvivor.Shared;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameMetaSystemGroup))]
    public partial struct LevelUpFlowSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<GameFlowState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

            Entity progressionEntity = SystemAPI.GetSingletonEntity<GameFlowState>();
            RefRW<PlayerExperience> experience = SystemAPI.GetComponentRW<PlayerExperience>(player);
            RefRW<GameFlowState> flow = SystemAPI.GetComponentRW<GameFlowState>(progressionEntity);
            RefRW<LevelUpState> levelUp = SystemAPI.GetComponentRW<LevelUpState>(progressionEntity);
            DynamicBuffer<LevelUpOption> options = SystemAPI.GetBuffer<LevelUpOption>(progressionEntity);
            DynamicBuffer<UpgradeDefinition> definitions = SystemAPI.GetBuffer<UpgradeDefinition>(progressionEntity);
            DynamicBuffer<UpgradeProgress> progress = SystemAPI.GetBuffer<UpgradeProgress>(player);
            DynamicBuffer<UnlockedAbility> unlocked = SystemAPI.GetBuffer<UnlockedAbility>(player);

            if (flow.ValueRO.Phase == GamePhase.Playing)
            {
                if (experience.ValueRO.PendingLevelUps <= 0)
                    return;

                bool generated = GenerateOptions(definitions, progress, unlocked, options, ref levelUp.ValueRW);
                
                if (!generated)
                    return;

                flow.ValueRW.Phase = GamePhase.LevelUp;
                return;
            }
            
            if (flow.ValueRO.Phase != GamePhase.LevelUp)
                return;

            int selectedIndex = levelUp.ValueRO.SelectedIndex;
            
            if (selectedIndex < 0)
                return;

            LevelUpOption selected = options[selectedIndex];

            ApplyUpgrade(selected, progress);

            experience.ValueRW.PendingLevelUps--;

            levelUp.ValueRW.SelectedIndex = -1;

            if (experience.ValueRO.PendingLevelUps > 0)
            {
                GenerateOptions(definitions, progress, unlocked, options, ref levelUp.ValueRW);
                
                return;
            }
            
            options.Clear();
            
            flow.ValueRW.Phase = GamePhase.Playing;

        }

        private bool GenerateOptions(
            DynamicBuffer<UpgradeDefinition> definitions,
            DynamicBuffer<UpgradeProgress> progresses,
            DynamicBuffer<UnlockedAbility> unlocked, 
            DynamicBuffer<LevelUpOption> options,
            ref LevelUpState levelUp)
        {
            options.Clear();

            var candidates = new NativeList<int>(definitions.Length, Allocator.Temp);

            for (int i = 0; i < definitions.Length; i++)
            {
                UpgradeDefinition definition = definitions[i];
                
                if (!IsAbilityUnlocked(unlocked, definition.Ability)) 
                    continue;

                int picks = GetPicks(progresses, definition.Ability, definition.Key);
                
                if (picks > definition.MaxPicks)
                    continue;
                
                candidates.Add(i);
            }

            if (candidates.Length == 0)
            {
                candidates.Dispose();

                return false;
            }

            uint randomState = math.max(1u, levelUp.RandomState);
            var random = new Random(randomState);
            int optionCount = math.min(3, candidates.Length);
            for (int i = 0; i < optionCount; i++)
            {
                int candidateIndex = random.NextInt(candidates.Length);
                int definitionIndex = candidates[candidateIndex];
                UpgradeDefinition definition = definitions[definitionIndex];
                int picks = GetPicks(progresses, definition.Ability, definition.Key);

                options.Add(new LevelUpOption
                {
                    Ability = definition.Ability,
                    Key =  definition.Key,
                    Bonus = definition.BonusPerPick,
                    CurrentPicks = picks,
                    MaxPicks =  definition.MaxPicks
                });
                
                candidates.RemoveAtSwapBack(candidateIndex);
            }

            levelUp.RandomState = random.state;

            return true;
        }

        private int GetPicks(DynamicBuffer<UpgradeProgress> progresses, AbilityId ability, UpgradeKey key)
        {
            for (int i = 0; i < progresses.Length; i++)
            {
                UpgradeProgress current = progresses[i];

                if (current.Ability == ability && current.Key == key)
                    return current.Picks;
            }

            return 0;
        }

        private void ApplyUpgrade(LevelUpOption option, DynamicBuffer<UpgradeProgress> progress)
        {
            for (int i = 0; i < progress.Length; i++)
            {
                UpgradeProgress current = progress[i];
                
                if (current.Ability != option.Ability)
                    continue;
                
                if (current.Key != option.Key)
                    continue;

                current.Picks++;
                current.TotalBonus += option.Bonus;
                progress[i] = current;
                
                return;
            }

            progress.Add(new UpgradeProgress
            {
                Ability = option.Ability,
                Key = option.Key,
                Picks = 1,
                TotalBonus = option.Bonus
            });
        }

        private bool IsAbilityUnlocked(DynamicBuffer<UnlockedAbility> unlocked, AbilityId definitionAbility)
        {
            foreach (var unlockedAbility in unlocked)
            {
                if (unlockedAbility.Value == definitionAbility)
                    return true;
            }

            return false;
        }
    }
}