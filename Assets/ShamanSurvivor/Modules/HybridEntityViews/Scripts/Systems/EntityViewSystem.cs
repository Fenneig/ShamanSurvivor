using UnityEngine;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Creates and destroys <see cref="EntityView"/> instances for entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An entity receives a view when it contains an <see cref="EntityViewPrefab"/>
    /// but does not yet have an <see cref="EntityViewReference"/>.
    /// </para>
    ///
    /// <para>
    /// When the <see cref="EntityViewPrefab"/> component is removed, the
    /// associated view is hidden, returned to the internal pool, and the
    /// <see cref="EntityViewReference"/> cleanup component is removed.
    /// </para>
    ///
    /// <para>
    /// View instances are pooled and reused to minimize allocations and
    /// instantiation overhead.
    /// </para>
    /// </remarks>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public sealed partial class EntityViewSystem : SystemBase
    {
        private EntityViewPool _pool;

        protected override void OnCreate()
        {
            _pool = new GameObject(nameof(EntityViewPool)).AddComponent<EntityViewPool>();
        }

        protected override void OnDestroy()
        {
            if (_pool != null)
                GameObject.Destroy(_pool.gameObject);
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(this.World.Unmanaged);

            foreach ((RefRO<EntityViewPrefab> prefabRef, Entity entity) in SystemAPI
                         .Query<RefRO<EntityViewPrefab>>()
                         .WithNone<EntityViewReference>()
                         .WithEntityAccess())
            {
                EntityView view = _pool.Rent(prefabRef.ValueRO.value.Value);

                ecb.AddComponent(entity, new EntityViewReference
                {
                    value = view
                });

                view.Show(entity, ecb);
            }

            foreach ((RefRO<EntityViewReference> viewRef, Entity entity) in SystemAPI
                         .Query<RefRO<EntityViewReference>>()
                         .WithNone<EntityViewPrefab>()
                         .WithEntityAccess())
            {
                EntityView view = viewRef.ValueRO.value.Value;

                view.Hide(entity, ecb);

                _pool.Return(view);

                ecb.RemoveComponent<EntityViewReference>(entity);
            }
        }
    }
}