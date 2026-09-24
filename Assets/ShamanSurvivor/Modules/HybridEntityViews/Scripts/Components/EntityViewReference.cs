using System;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Stores the <see cref="EntityView"/> currently assigned to an entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an internal runtime component used by
    /// <see cref="EntityViewSystem"/> to track the active view associated with
    /// an entity.
    /// </para>
    ///
    /// <para>
    /// The component is added automatically when a view is created and removed
    /// after the view has been returned to the pool.
    /// </para>
    ///
    /// <para>
    /// As an <see cref="ICleanupComponentData"/>, it remains available after the
    /// entity is destroyed, allowing the system to properly hide and recycle the
    /// associated view before the component is removed.
    /// </para>
    /// </remarks>
    [Serializable]
    internal struct EntityViewReference : ICleanupComponentData
    {
        /// <summary>
        /// Reference to the active <see cref="EntityView"/> instance.
        /// </summary>
        internal UnityObjectRef<EntityView> value;
    }
}