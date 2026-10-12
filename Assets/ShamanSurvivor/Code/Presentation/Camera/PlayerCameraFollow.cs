using ShamanSurvivor.Runtime;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class PlayerCameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _offset;

        [SerializeField]
        private bool _smoothFollow = false;

        [SerializeField, Min(0f), ShowIf("_smoothFollow")]
        private float _followSpeed = 10f;

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _playerQuery;

        private Entity _player = Entity.Null;

        private void LateUpdate()
        {
            if (!TryGetPlayer())
                return;

            LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_player);

            Vector3 position = transform.position;

            Vector3 target = new Vector3(playerTransform.Position.x + _offset.x, playerTransform.Position.y + _offset.y, position.z);

            if (_smoothFollow)
            {
                float t = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);

                transform.position = Vector3.Lerp(position, target, t);
            }
            else
            {
                transform.position = target;
            }
        }

        private bool TryGetPlayer()
        {
            if (_player != Entity.Null)
                return true;
            
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
                return false;

            if (_world != world)
            {
                _world = world;
                _entityManager = world.EntityManager;
                _playerQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<LocalTransform>());
                _player = Entity.Null;
            }

            if (_player != Entity.Null && _entityManager.Exists(_player) && _entityManager.HasComponent<LocalTransform>(_player))
                return true;

            if (_playerQuery.IsEmptyIgnoreFilter)
                return false;

            _player = _playerQuery.GetSingletonEntity();

            return true;
        }
    }
}