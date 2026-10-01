using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Projectile Ability Catalog Config", menuName = "Shaman Survivor/Abilities/Projectile Ability Catalog")]
    public class AbilityCatalogConfig : GameConfig
    {
        [SerializeField] public AbilityConfig[] Abilities;
    }
}