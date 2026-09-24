using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    [Serializable]
    public sealed class IntRandom : IIntSource
    {
        public int Value => Random.Range(this.minValue, this.maxValue);

#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        [SerializeField]
        private int minValue;

#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        [SerializeField]
        private int maxValue = 1;
    }
}