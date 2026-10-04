using ShamanSurvivor.Shared;
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    public abstract class AbilityConfig : GameConfig
    {
        [Header("Identity")]
        public AbilityId Ability;
        [Min(0f)]
        public float Damage = 10f;
        public DamageElement DamageElement;
        [Min(0f)]
        public float Range = 8f;
        [Min(0.01f)]
        public float AttackInterval = 1f;
        [Tooltip("Может ли способность появляться как карточка разблокировки.")]
        public bool CanUnlock = true;
    }
}