using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class ExperienceDropSettingsAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _gameObject;

        public class Baker : Baker<ExperienceDropSettingsAuthoring>
        {
            public override void Bake(ExperienceDropSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ExperienceDropSettings { OrbPrefab = GetEntity(authoring._gameObject, TransformUsageFlags.Dynamic) });
            }
        }
    }
}