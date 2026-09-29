using ShamanSurvivor.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameMovementSystemGroup))]
    public partial struct ExperiencePickupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<PlayerExperience>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            float2 playerPosition = SystemAPI.GetComponent<LocalTransform>(player).Position.xy;
            DynamicBuffer<PassiveProgress> passiveProgresses = SystemAPI.GetBuffer<PassiveProgress>(player);
            float experienceBonus = PassiveProgressUtility.GetBonus(passiveProgresses, GlobalPassiveId.ExperienceGain);
            float radiusBonus = PassiveProgressUtility.GetBonus(passiveProgresses, GlobalPassiveId.PickupRadius);
            float deltaTime = SystemAPI.Time.DeltaTime;
            int collectedExperience = 0;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (transform, orb, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperienceOrb>>()
                         .WithEntityAccess())
            {
                float2 position = transform.ValueRO.Position.xy;
                float2 toPlayer = playerPosition - position;
                float distanceSq = math.lengthsq(toPlayer);
                float collectRadiusSq = orb.ValueRO.CollectRadius * orb.ValueRO.CollectRadius;

                if (distanceSq <= collectRadiusSq)
                {
                    collectedExperience += orb.ValueRO.Value;
                    
                    ecb.DestroyEntity(entity);
                    
                    continue;
                }

                float effectRadius = orb.ValueRO.MagnetRadius * (1 + radiusBonus);
                float magnetRadiusSq = effectRadius * effectRadius;

                if (distanceSq > magnetRadiusSq)
                    continue;

                float distance = math.sqrt(distanceSq);
                
                if (distance <= 0.0001f)
                    continue;

                float moveDistance = orb.ValueRO.MoveSpeed * deltaTime;
                moveDistance = math.min(moveDistance, distance);
                position += toPlayer / distanceSq * moveDistance;
                float3 newPosition = transform.ValueRO.Position;
                newPosition.xy = position;
                transform.ValueRW.Position = newPosition;
            }
            
            if (collectedExperience <= 0)
                return;
            
            RefRW<PlayerExperience> experience = SystemAPI.GetComponentRW<PlayerExperience>(player);

            float modifiedExperience = collectedExperience * (1f + experienceBonus);

            modifiedExperience += experience.ValueRO.Reminder;
            
            int wholeExperience = (int)math.floor(modifiedExperience);
            experience.ValueRW.Reminder = modifiedExperience - wholeExperience;
            
            AddExperience(ref experience.ValueRW, wholeExperience);
        }

        private void AddExperience(ref PlayerExperience experience, int value)
        {
            experience.Current += value;
            while (experience.Current >= experience.Required)
            {
                experience.Current -= experience.Required;
                experience.Level++;
                experience.PendingLevelUps++;
                experience.Required = GetRequiredExperience(experience.Level);
            }
        }

        private int GetRequiredExperience(int level)
        {
            //TODO: имплементировать нормальную кривую
            return 5 + (level - 1) * 3;
        }
    }
}