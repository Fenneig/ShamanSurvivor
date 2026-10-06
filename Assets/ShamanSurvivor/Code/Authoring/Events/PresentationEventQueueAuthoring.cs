using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class PresentationEventQueueAuthoring : MonoBehaviour
    {
        private class Baker : Baker<PresentationEventQueueAuthoring>
        {
            public override void Bake(PresentationEventQueueAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new PresentationEventQueueTag());
                AddBuffer<ChainLightningVisualEvent>(entity);
            }
        }
    }
}