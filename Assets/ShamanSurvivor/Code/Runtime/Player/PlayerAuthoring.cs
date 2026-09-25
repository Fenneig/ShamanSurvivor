using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime.Player
{
    public sealed class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;

        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new PlayerTag());
                
                AddComponent(entity, new PlayerMovement{Speed = authoring._moveSpeed});

                AddComponent(entity, new PlayerInput { Move = default });
            }
        }
    }
}