using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class ExperienceOrbAuthoring : MonoBehaviour
    {
        [SerializeField] private XpOrbConfig _config;

        public class Baker : Baker<ExperienceOrbAuthoring>
        {
            public override void Bake(ExperienceOrbAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new ExperienceOrb
                    {
                        Value = 0,
                        MagnetRadius = authoring._config.MagnetRadius,
                        CollectRadius = authoring._config.CollectRadius,
                        MoveSpeed = authoring._config.MoveSpeed
                    });
            }
        }
    }
}