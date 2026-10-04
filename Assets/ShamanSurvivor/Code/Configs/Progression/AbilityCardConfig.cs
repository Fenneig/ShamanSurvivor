using System.Collections.Generic;
using ShamanSurvivor.Shared;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "AbilityCardConfig", menuName = "Shaman Survivor/UI/Ability card config")]
    public class AbilityCardConfig : GameConfig
    {
        public AbilityId Ability;
        public string DisplayName;
        [TextArea] public string UnlockDescription;
        public Color NameColor;
        public Sprite Icon;
        public Sprite Frame;
#if ODIN_INSPECTOR
        [TableList]
#endif
        public List<LevelUpCardCatalog.AbilityKeyEntry> Keys;
    }
}