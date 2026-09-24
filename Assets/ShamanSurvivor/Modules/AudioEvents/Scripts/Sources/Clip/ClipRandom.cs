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
    public sealed class ClipRandom : IClipSource
    {
        [SerializeField]
        private AudioClip[] clips;

        public AudioClip Value
        {
            get
            {
                if (clips == null || clips.Length == 0)
                    return null;

                // Попробуем несколько раз найти ненулевой клип
                foreach (var t in clips)
                {
                    int randomIndex = Random.Range(0, clips.Length);
                    var clip = clips[randomIndex];
                    if (clip != null)
                        return clip;
                }

                return null;
            }
        }

        public float MaxLength
        {
            get
            {
                if (clips == null || clips.Length == 0)
                    return 0f;

                float maxLength = 0f;

                foreach (var clip in clips)
                    if (clip != null && clip.length > maxLength)
                        maxLength = clip.length;

                return maxLength;
            }
        }
    }
}