using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using ShamanSurvivor.Shared;
using Random = Unity.Mathematics.Random;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameMetaSystemGroup))]
    public partial struct LevelUpFlowSystem : ISystem
    {
        private struct Candidate
        {
            public LevelUpOptionType Type;
            public int DefinitionIndex;
        }
        
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
            DynamicBuffer<PassiveDefinition> passiveDefinitions = SystemAPI.GetBuffer<PassiveDefinition>(progressionEntity);
            DynamicBuffer<PassiveProgress> passiveProgress = SystemAPI.GetBuffer<PassiveProgress>(player);

            if (flow.ValueRO.Phase == GamePhase.Playing)
            {
                if (experience.ValueRO.PendingLevelUps <= 0)
                    return;

                bool generated = GenerateOptions(definitions, progress, unlocked, options, passiveDefinitions, passiveProgress, ref levelUp.ValueRW);
                
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

            if (selected.Type == LevelUpOptionType.AbilityUpgrade)
                ApplyUpgrade(selected, progress);
            else if (selected.Type == LevelUpOptionType.GlobalPassive)
                ApplyPassive(selected, passiveProgress);

            experience.ValueRW.PendingLevelUps--;

            levelUp.ValueRW.SelectedIndex = -1;

            if (experience.ValueRO.PendingLevelUps > 0)
            {
                GenerateOptions(definitions, progress, unlocked, options, passiveDefinitions, passiveProgress, ref levelUp.ValueRW);
                
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
            DynamicBuffer<PassiveDefinition> passiveDefinitions,
            DynamicBuffer<PassiveProgress> passiveProgress,
            ref LevelUpState levelUp)
        {
            options.Clear();

            var candidates = new NativeList<Candidate>(definitions.Length + passiveDefinitions.Length, Allocator.Temp);

            for (int i = 0; i < definitions.Length; i++)
            {
                UpgradeDefinition definition = definitions[i];
                
                if (!IsAbilityUnlocked(unlocked, definition.Ability)) 
                    continue;

                int picks = GetPicks(progresses, definition.Ability, definition.Key);
                
                if (picks >= definition.MaxPicks)
                    continue;
                
                candidates.Add(new Candidate{DefinitionIndex = i, Type = LevelUpOptionType.AbilityUpgrade});
            }

            for (int i = 0; i < passiveDefinitions.Length; i++)
            {
                PassiveDefinition passiveDefinition = passiveDefinitions[i];
                
                int picks = GetPicks(passiveProgress, passiveDefinition.Passive);
                
                if (picks >= passiveDefinition.MaxPicks)
                    continue;
                
                candidates.Add(new Candidate{DefinitionIndex = i, Type = LevelUpOptionType.GlobalPassive});
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
                Candidate candidate = candidates[candidateIndex];

                switch (candidate.Type)
                {
                    case LevelUpOptionType.AbilityUpgrade:
                        UpgradeDefinition abilityDefinition = definitions[candidate.DefinitionIndex];
                        int abilityPicks = GetPicks(progresses, abilityDefinition.Ability, abilityDefinition.Key);
                        options.Add(new LevelUpOption
                        {
                            Type = LevelUpOptionType.AbilityUpgrade,
                            Ability = abilityDefinition.Ability,
                            Key =  abilityDefinition.Key,
                            Bonus = abilityDefinition.BonusPerPick,
                            CurrentPicks = abilityPicks,
                            MaxPicks =  abilityDefinition.MaxPicks
                        });
                        break;
                    case LevelUpOptionType.GlobalPassive:
                        PassiveDefinition passiveDefinition = passiveDefinitions[candidate.DefinitionIndex];
                        int passivePicks = GetPicks(passiveProgress, passiveDefinition.Passive);
                        options.Add(new LevelUpOption
                        {
                            Type = LevelUpOptionType.GlobalPassive,
                            Passive = passiveDefinition.Passive,
                            Bonus = passiveDefinition.BonusPerPick,
                            CurrentPicks = passivePicks,
                            MaxPicks =  passiveDefinition.MaxPicks
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                candidates.RemoveAtSwapBack(candidateIndex);
            }

            levelUp.RandomState = random.state;
            levelUp.Revision++;

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


        private int GetPicks(DynamicBuffer<PassiveProgress> progresses, GlobalPassiveId passive)
        {
            for (int i = 0; i < progresses.Length; i++)
            {
                PassiveProgress current = progresses[i];

                if (current.Passive == passive)
                    return current.Picks;
            }

            return 0;
        }

        private void ApplyUpgrade(LevelUpOption selected, DynamicBuffer<UpgradeProgress> progress)
        {
            for (int i = 0; i < progress.Length; i++)
            {
                UpgradeProgress current = progress[i];
                
                if (current.Ability != selected.Ability)
                    continue;
                
                if (current.Key != selected.Key)
                    continue;

                current.Picks++;
                current.TotalBonus += selected.Bonus;
                progress[i] = current;
                
                return;
            }

            progress.Add(new UpgradeProgress
            {
                Ability = selected.Ability,
                Key = selected.Key,
                Picks = 1,
                TotalBonus = selected.Bonus
            });
        }

        private void ApplyPassive(LevelUpOption selected, DynamicBuffer<PassiveProgress> progress)
        {
            for (int i = 0; i < progress.Length; i++)
            {
                PassiveProgress current = progress[i];
                
                if (current.Passive != selected.Passive)
                    continue;

                current.Picks++;
                current.TotalBonus += selected.Bonus;
                progress[i] = current;
                
                return;
            }

            progress.Add(new PassiveProgress
            {
                Passive =  selected.Passive,
                Picks = 1,
                TotalBonus = selected.Bonus
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