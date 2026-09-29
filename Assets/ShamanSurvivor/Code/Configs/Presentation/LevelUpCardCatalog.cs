using System;
using System.Collections.Generic;
using ShamanSurvivor.Shared;
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "LevelUpCardCatalog", menuName = "Shaman Survivor/UI/Level Up Card Catalog")]
    public sealed class LevelUpCardCatalog : ScriptableObject
    {
        [Serializable]
        public struct AbilityVisual
        {
            public AbilityId Ability;
            public string DisplayName;
            public Color NameColor;
            public Sprite Icon;
            public Sprite Frame;
        }

        [Serializable]
        public struct KeyVisual
        {
            public UpgradeKey Key;
            public string DisplayName;
            [TextArea]
            public string DescriptionFormat;
        }
        
        [Serializable]
        public struct PassiveVisual
        {
            public GlobalPassiveId Passive;
            public string DisplayName;
            public string KeyName;
            public Color NameColor;
            [TextArea]
            public string DescriptionFormat;
            public Sprite Icon;
            public Sprite Frame;
        }

        [SerializeField] private List<AbilityVisual> _abilities = new();
        [SerializeField] private List<KeyVisual> _keys = new();
        [SerializeField] private List<PassiveVisual> _passiveVisuals = new();

        public bool TryGetAbility(AbilityId ability, out AbilityVisual result)
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

        public bool TryGetKey(UpgradeKey key, out KeyVisual result)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].Key != key)
                    continue;

                result = _keys[i];
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGetPassive(GlobalPassiveId passive, out PassiveVisual result)
        {
            for (int i = 0; i < _passiveVisuals.Count; i++)
            {
                if (_passiveVisuals[i].Passive != passive)
                    continue;

                result = _passiveVisuals[i];
                return true;
            }

            result = default;
            return false;
        }
    }
}