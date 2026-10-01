using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class DirectImpactAuthoring : MonoBehaviour
    {
        public class DirectImpactBaker : Baker<DirectImpactAuthoring>
        {
            public override void Bake(DirectImpactAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<DirectImpact>(entity);
            }
        }
    }
}