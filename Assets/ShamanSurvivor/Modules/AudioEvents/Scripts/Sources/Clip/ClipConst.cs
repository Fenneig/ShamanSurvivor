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
    public sealed class ClipConst : IClipSource
    {
        [SerializeField]
        private AudioClip clip;

        public AudioClip Value => this.clip;

        public float MaxLength => this.clip != null ? clip.length : 0;
    }
}