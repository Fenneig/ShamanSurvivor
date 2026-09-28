using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime.Player
{
    public sealed class PlayerAuthoring : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed;
        [Header("Body")] 
        [SerializeField] private float _hitRadius;
        [Header("Health")] 
        [SerializeField] private float _maxHealth;

        [Header("Experience")]
        [Min(1)]
        [SerializeField] private int _firstLevelExperience = 5;
        
        
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new PlayerTag());
                AddComponent(entity, new PlayerMovement{Speed = authoring._moveSpeed});
                AddComponent(entity, new PlayerInput { Move = default });
                AddComponent(entity, new Health { Max = authoring._maxHealth, Current = authoring._maxHealth });
                AddComponent(entity, new HitRadius { Value = authoring._hitRadius });
                AddBuffer<DamageEvent>(entity);
                AddComponent(entity, new PlayerExperience
                {
                    Current = 0,
                    Level = 1,
                    Required = authoring._firstLevelExperience,
                    PendingLevelUps = 0
                });
            }
        }
    }
}