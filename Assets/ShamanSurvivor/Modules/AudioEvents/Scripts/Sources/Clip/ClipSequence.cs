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
    public sealed class ClipSequence : IClipSource
    {
        [SerializeField]
        private AudioClip[] clips;

        private int _pointer;

        public AudioClip Value
        {
            get
            {
                if (clips == null || clips.Length == 0)
                    return null;

                // Если массив изменился и pointer вышел за границы
                if (_pointer >= clips.Length)
                    _pointer = 0;

                // Попробуем найти следующий ненулевой клип
                int startIndex = _pointer;

                for (int i = 0; i < clips.Length; i++)
                {
                    int index = (_pointer + i) % clips.Length;
                    var clip = clips[index];

                    if (clip != null)
                    {
                        _pointer = (index + 1) % clips.Length;
                        return clip;
                    }
                }

                // Если все null
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

                for (int i = 0; i < clips.Length; i++)
                {
                    var clip = clips[i];
                    if (clip != null && clip.length > maxLength)
                        maxLength = clip.length;
                }

                return maxLength;
            }
        }
    }
}