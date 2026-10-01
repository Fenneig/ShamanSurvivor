using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        [SerializeField] private EnemyConfig _config;

        public class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyTag());
                AddComponent(entity, new EnemyMovement { Speed = authoring._config.MoveSpeed });
                AddComponent(entity, new Health { Max = authoring._config.MaxHealth, Current = authoring._config.MaxHealth });
                AddComponent(entity, new HitRadius { Value = authoring._config.HitRadius });
                AddComponent(entity, new ContactDamage{ Damage = authoring._config.ContactDamage, Interval = authoring._config.ContactDamageInterval });
                AddComponent(entity, new DestroyOnDeath());
                AddBuffer<DamageEvent>(entity);
                AddComponent(entity, new EnemySeparation
                {
                    SearchRadius = authoring._config.SeparationSearchRadius,
                    PersonalSpace = authoring._config.PersonalSpace,
                    Strength = authoring._config.SeparationStrength
                });
                AddComponent(entity, new ExperienceReward { Value = authoring._config.ExperienceReward });
                AddComponent(entity, new Burning
                {
                    Source = Entity.Null,
                    DamagePerTick = 0,
                    TickInterval = 0,
                    TickTimer = 0,
                    RemainingDuration = 0
                });
            }
        }
    }
}