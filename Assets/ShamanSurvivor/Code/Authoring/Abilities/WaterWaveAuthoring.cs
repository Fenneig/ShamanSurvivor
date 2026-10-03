using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class WaterWaveAuthoring : MonoBehaviour
    {
        [SerializeField] private WaterWaveConfig _config;
        [SerializeField] private float _hitRadius;
        
        public class Baker : Baker<WaterWaveAuthoring>
        {
            public override void Bake(WaterWaveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ProjectileTag());
                AddComponent(entity,
                    new Projectile
                    {
                        Source = Entity.Null,
                        Direction = float2.zero,
                        Speed = 0f,
                        Damage = 0f,
                        RemainingLifetime = 0f,
                        Element = DamageElement.Water
                    });             
                AddComponent(entity, new ProjectileBody
                {
                    HitRadius = authoring._hitRadius
                });

                AddComponent(entity, new ProjectileModifierSnapshot
                {
                    SizeMultiplier = 1f,
                    DurationMultiplier = 1f,
                    EffectStrengthMultiplier = 1f,
                    DamageMultiplier = 1f
                });              
                AddComponent(entity, new PiercingProjectile());
                AddComponent(entity, new WaterWave
                {
                    HalfDepth = authoring._config.HalfDepth,
                    HalfWidth = authoring._config.HalfWidth,
                    KnockbackDistance = authoring._config.KnockbackDistance,
                    KnockbackSpeed = authoring._config.ProjectileSpeed
                });
                AddBuffer<ProjectileHitHistory>(entity);

            }
        }
        
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_config == null)
                return;

            const int segments = 48;

            Vector3 previous = GetPoint(0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle =
                    i / (float)segments *
                    Mathf.PI * 2f;

                Vector3 current =
                    GetPoint(angle);

                Gizmos.DrawLine(
                    previous,
                    current);

                previous = current;
            }

            Gizmos.DrawLine(transform.position, transform.position + transform.right * _config.HalfDepth * 2f);
        }

        private Vector3 GetPoint(float angle)
        {
            Vector3 local = new Vector3(Mathf.Cos(angle) * _config.HalfDepth, Mathf.Sin(angle) * _config.HalfWidth, 0f);
            return transform.TransformPoint(local);
        }
#endif
    }
}