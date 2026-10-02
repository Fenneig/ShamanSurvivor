using System;
using System.Collections.Generic;
using ShamanSurvivor.Shared;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "LevelUpCardCatalog", menuName = "Shaman Survivor/UI/Level Up Card Catalog")]
    public sealed class LevelUpCardCatalog : ScriptableObject
    {
        [Serializable]
        public struct AbilityEntry
        {
            public AbilityId Ability;
            public string DisplayName;
            [TextArea] 
            public string UnlockDescription;
            public Color NameColor;
            public Sprite Icon;
            public Sprite Frame;

            [Tooltip("Может ли способность появляться как карточка разблокировки.")]
            public bool CanUnlock;

#if ODIN_INSPECTOR
            [TableList]
#endif
            public List<AbilityKeyEntry> Keys;
        }

        [Serializable]
        public struct AbilityKeyEntry
        {
            public UpgradeKey Key;
            public string DisplayName;
            [TextArea]
            public string DescriptionFormat;

            public float BonusPerPick;
            [Min(1)]
            public int MaxPicks;
        }
        
        [Serializable]
        public struct PassiveEntry
        {
            public GlobalPassiveId Passive;
            public string DisplayName;
            public string KeyName;
            public Color NameColor;
            [TextArea]
            public string DescriptionFormat;
            public Sprite Icon;
            public Sprite Frame;
            public float BonusPerPick;
            [Min(1)]
            public int MaxPicks;
        }

        [SerializeField] private List<AbilityEntry> _abilities = new();
        [SerializeField] private List<PassiveEntry> _passives = new();

        public List<AbilityEntry> Abilities => _abilities;

        public List<PassiveEntry> Passives => _passives;

        public bool TryGetAbility(AbilityId ability, out AbilityEntry result)
        {
            for (int i = 0; i < _abilities.Count; i++)
            {
                if (_abilities[i].Ability != ability)
                    continue;

                result = _abilities[i];
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGetKey(AbilityId ability, UpgradeKey key, out AbilityKeyEntry result)
        {
            foreach (var abilityEntry in _abilities)
            {
                if (abilityEntry.Ability != ability)
                    continue;
                
                foreach (var keyEntry in abilityEntry.Keys)
                {
                    if (keyEntry.Key != key)
                        continue;

                    result = keyEntry;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public bool TryGetPassive(GlobalPassiveId passive, out PassiveEntry result)
        {
            for (int i = 0; i < _passives.Count; i++)
            {
                if (_passives[i].Passive != passive)
                    continue;

                result = _passives[i];
                return true;
            }

            result = default;
            return false;
        }
    }
}