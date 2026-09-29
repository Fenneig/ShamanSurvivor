using System;
using System.Collections.Generic;
using ShamanSurvivor.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
#if ODIN_INSPECTOR
#endif

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Shaman Survivor/Progression Config")]
    public class ProgressionConfig : GameConfig
    {
        [Serializable]
        public struct UpgradeEntry
        {
            public AbilityId Ability;
            public UpgradeKey Key;

            public float BonusPerPick;

            [Min(1)] public int MaxPicks;
        }
        
        [Serializable]
        public struct PassiveEntry
        {
            public GlobalPassiveId Passive;
            public float BonusPerPick;
            [Min(1)] public int MaxPicks;
        }

        [Min(1)]
        public uint RandomSeed = 1;
#if ODIN_INSPECTOR
        [TableList]
#endif
        public List<UpgradeEntry> Upgrades = new();
#if ODIN_INSPECTOR
        [TableList]
#endif
        public List<PassiveEntry> Passives = new();
    }
}