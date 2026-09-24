using System;
using System.Collections.Generic;
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
    public sealed class ClipRandomNonRepeat : IClipSource
    {
        [SerializeField]
        private AudioClip[] clips;

        private readonly List<int> _queue = new();

        public AudioClip Value
        {
            get
            {
                this.EnsureQueueFilled();

                if (_queue.Count == 0)
                    return null;

                int pick = Random.Range(0, _queue.Count);
                int clipIndex = _queue[pick];

                int last = _queue.Count - 1;
                _queue[pick] = _queue[last];
                _queue.RemoveAt(last);

                return clips[clipIndex];
            }
        }

        public float MaxLength
        {
            get
            {
                if (clips == null || clips.Length == 0)
                    return 0f;

                float max = 0f;
                foreach (var c in clips)
                    if (c != null && c.length > max)
                        max = c.length;

                return max;
            }
        }

        private void EnsureQueueFilled()
        {
            if (_queue.Count > 0)
                return;

            if (clips == null || clips.Length == 0)
                return;

            for (int i = 0; i < clips.Length; i++)
                if (clips[i] != null)
                    _queue.Add(i);
        }
    }
}