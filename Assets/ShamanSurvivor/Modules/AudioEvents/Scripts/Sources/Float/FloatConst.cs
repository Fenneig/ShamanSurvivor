using System;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif


namespace Modules.AudioEvents
{
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    [Serializable]
    public sealed class FloatConst : IFloatSource
    {
        [SerializeField]
        private float value;

        public FloatConst()
        {
        }

        public FloatConst(float value) => this.value = value;

        public float Value => this.value;
    }
}