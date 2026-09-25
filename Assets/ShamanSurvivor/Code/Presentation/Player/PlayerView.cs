using Unity.Entities;
using Unity.Entities.HybridViews.Modules.HybridEntityViews;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Code.Presentation.Player
{
    public sealed class PlayerView : EntityView
    {
        private Entity _entity;
        private EntityManager _entityManager;
        
        protected override void Show(Entity entity, EntityCommandBuffer ecb)
        {
            _entity = entity;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            SyncTransform();
        }

        private void LateUpdate()
        {
            SyncTransform();
        }

        private void SyncTransform()
        {
            if (_entity == Entity.Null)
                return;
            
            if (!_entityManager.Exists(_entity))
                return;
            
            if (!_entityManager.HasComponent<LocalTransform>(_entity))
                return;
            
            LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(_entity);

            var position = localTransform.Position;
            var rotation = localTransform.Rotation.value;
            
            transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, position.z),
                new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w));

        }

        protected override void Hide(Entity entity, EntityCommandBuffer ecb)
        {
            _entity = Entity.Null;
        }
    }
}