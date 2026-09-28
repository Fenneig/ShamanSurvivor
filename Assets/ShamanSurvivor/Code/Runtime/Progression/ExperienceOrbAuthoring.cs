using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime
{
    public class ExperienceOrbAuthoring : MonoBehaviour
    {
        [Header("Pickup")]
        [Min(0f)]
        [SerializeField] private float _magnetRadius;
        [Min(0f)] 
        [SerializeField] private float _collectRadius;
        [Min(0f)]
        [SerializeField] private float _moveSpeed;

        public class Baker : Baker<ExperienceOrbAuthoring>
        {
            public override void Bake(ExperienceOrbAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new ExperienceOrb
                    {
                        Value = 0,
                        MagnetRadius = authoring._magnetRadius,
                        CollectRadius = authoring._collectRadius,
                        MoveSpeed = authoring._moveSpeed
                    });
            }
        }
    }
}