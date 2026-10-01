using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class LavaEruptionAuthoring : MonoBehaviour
    {
        [SerializeField] private LavaEruptionConfig _config;
        
        public class LavaProjectileBaker : Baker<LavaEruptionAuthoring>
        {
            public override void Bake(LavaEruptionAuthoring authoring)
            {
                LavaEruptionConfig config = authoring._config;
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LavaImpact
                {
                    ExplosionRadius = config.ExplosionRadius,
                    BurnDamagePerTick = config.BurnDamagePerTick,
                    BurnDuration = config.BurnDuration,
                    BurnTickInterval = config.BurnTickInterval,
                    BurningTargetDamageMultiplier = config.BurningTargetDamageMultiplier
                });
            }
        }
    }
}