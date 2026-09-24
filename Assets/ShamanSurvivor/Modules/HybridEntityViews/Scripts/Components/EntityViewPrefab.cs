using System;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Specifies the <see cref="EntityView"/> prefab used to represent an entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an entity contains this component, <see cref="EntityViewSystem"/>
    /// rents an instance of the specified prefab from the internal pool and
    /// associates it with the entity.
    /// </para>
    ///
    /// <para>
    /// Removing this component causes the associated view to be hidden and
    /// returned to the pool.
    /// </para>
    ///
    /// <para>
    /// This component is typically added during baking using
    /// <see cref="EntityViewPrefabAuthoring"/>.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct EntityViewPrefab : IComponentData
    {
        /// <summary>
        /// Reference to the <see cref="EntityView"/> prefab.
        /// </summary>
        public UnityObjectRef<EntityView> value;
    }
}