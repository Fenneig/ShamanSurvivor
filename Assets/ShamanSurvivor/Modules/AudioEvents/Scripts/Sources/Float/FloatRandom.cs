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
    public sealed class FloatRandom : IFloatSource
    {
        public float Value => Random.Range(this.minValue, this.maxValue);

#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        [SerializeField]
        private float minValue;

#if ODIN_INSPECTOR
        [HorizontalGroup]
#endif
        [SerializeField]
        private float maxValue = 1;
    }
}