using UnityEngine;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Base class for GameObject-based views representing ECS entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="EntityView"/> is the presentation layer of an ECS entity.
    /// Gameplay logic and state remain in ECS, while this class is responsible
    /// only for visual representation using a <see cref="MonoBehaviour"/>.
    /// </para>
    ///
    /// <para>
    /// Views are managed automatically by <c>EntityViewSystem</c>. When an entity
    /// contains an <see cref="EntityViewPrefab"/> component, a view instance is
    /// rented from an internal pool and associated with the entity. When the
    /// component is removed, the view is hidden and returned to the pool.
    /// </para>
    ///
    /// <para>
    /// View instances are reused. Do not assume that a particular instance will
    /// always represent the same entity. Any entity-specific state should be
    /// initialized in <see cref="Show"/> and cleaned up in <see cref="Hide"/>.
    /// </para>
    /// </remarks>
    public abstract class EntityView : MonoBehaviour
    {
        /// <summary>
        /// Gets the prefab this instance was created from.
        /// </summary>
        /// <remarks>
        /// Used internally by the pooling system to return this view to the
        /// correct pool. User code should not modify this property.
        /// </remarks>
        internal EntityView prefab;

        /// <summary>
        /// Called after this view has been assigned to an entity and activated.
        /// </summary>
        /// <param name="entity">
        /// The ECS entity represented by this view.
        /// </param>
        /// <param name="ecb">
        /// The <see cref="EntityCommandBuffer"/> for recording structural ECS
        /// changes related to this view.
        /// </param>
        /// <remarks>
        /// This method is called exactly once each time the view is rented from
        /// the pool. Override it to initialize visuals, subscribe to events,
        /// cache entity data, or add/remove ECS components through the provided
        /// command buffer.
        /// </remarks>
        protected internal abstract void Show(Entity entity, EntityCommandBuffer ecb);

        /// <summary>
        /// Called before this view is hidden and returned to the pool.
        /// </summary>
        /// <param name="entity">
        /// The ECS entity that was represented by this view.
        /// </param>
        /// <param name="ecb">
        /// The <see cref="EntityCommandBuffer"/> for recording structural ECS
        /// changes related to this view.
        /// </param>
        /// <remarks>
        /// Override this method to undo any work performed in
        /// <see cref="Show"/>, such as unsubscribing from events, stopping
        /// effects, or removing temporary ECS components.
        /// </remarks>
        protected internal abstract void Hide(Entity entity, EntityCommandBuffer ecb);
    }
}