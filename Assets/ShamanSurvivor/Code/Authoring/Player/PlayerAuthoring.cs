using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using ShamanSurvivor.Shared;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public sealed class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new PlayerTag());
                AddComponent(entity, new PlayerMovement{Speed = authoring._config.MoveSpeed});
                AddComponent(entity, new PlayerInput { Move = default });
                AddComponent(entity, new Health { Max = authoring._config.MaxHealth, Current = authoring._config.MaxHealth });
                AddComponent(entity, new HitRadius { Value = authoring._config.HitRadius });
                AddBuffer<DamageEvent>(entity);
                AddComponent(entity, new PlayerExperience
                {
                    Current = 0,
                    Level = 1,
                    Required = authoring._config.FirstLevelExperience,
                    PendingLevelUps = 0,
                    Reminder = 0f
                });
                AddBuffer<UpgradeProgress>(entity);
                DynamicBuffer<AbilityState> abilities = AddBuffer<AbilityState>(entity);
                abilities.Add(new AbilityState { Ability = AbilityId.Lightning, CooldownRemaining = 0 });
                AddBuffer<PassiveProgress>(entity);
            }
        }
    }
}