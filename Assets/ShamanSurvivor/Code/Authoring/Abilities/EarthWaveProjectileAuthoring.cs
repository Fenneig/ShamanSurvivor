using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class EarthWaveProjectileAuthoring : MonoBehaviour
    {
        [SerializeField] private EarthWaveConfig _config;

        private sealed class Baker : Baker<EarthWaveProjectileAuthoring>
        {
            public override void Bake(EarthWaveProjectileAuthoring authoring)
            {
                if (authoring._config == null)
                    return;

                DependsOn(authoring._config);

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PiercingProjectile>(entity);
                AddComponent(entity, new EarthWave
                {
                    HalfWidth = authoring._config.HalfWidth,
                    HalfDepth = authoring._config.HalfDepth,
                    SlowAmount = authoring._config.SlowAmount,
                    SlowDuration = authoring._config.SlowDuration
                });

                AddBuffer<ProjectileHitHistory>(entity);
            }
        }
    }
}