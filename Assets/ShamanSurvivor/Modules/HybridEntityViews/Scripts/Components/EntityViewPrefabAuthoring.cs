using UnityEngine;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Authoring component that assigns an <see cref="EntityView"/> prefab to an entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During baking, this component adds an <see cref="EntityViewPrefab"/>
    /// component containing the selected <see cref="EntityView"/> prefab.
    /// </para>
    ///
    /// <para>
    /// At runtime, <see cref="EntityViewSystem"/> uses this prefab to create,
    /// pool, and manage the visual representation of the entity.
    /// </para>
    /// </remarks>
    [AddComponentMenu("Entities/HybridViews/EntityViewPrefab")]
    public sealed class EntityViewPrefabAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Gets the <see cref="EntityView"/> prefab assigned to this authoring component.
        /// </summary>
        public EntityView Value => _value;

        [SerializeField]
        private EntityView _value;

        /// <summary>
        /// Bakes an <see cref="EntityViewPrefab"/> component for the entity.
        /// </summary>
        public sealed class Baker : Baker<EntityViewPrefabAuthoring>
        { 
            /// <inheritdoc />
            public override void Bake(EntityViewPrefabAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new EntityViewPrefab
                {
                    value = authoring._value
                });
            }
        }
    }
}