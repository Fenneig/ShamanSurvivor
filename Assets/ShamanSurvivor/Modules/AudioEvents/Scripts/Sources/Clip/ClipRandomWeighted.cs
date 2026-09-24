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
    public sealed class ClipRandomWeighted : IClipSource, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct Entry
        {
            public int weight;
            public AudioClip value;
        }

        [SerializeField]
        private Entry[] entries;

        // Сумма весов только валидных (weight > 0 && value != null)
        [NonSerialized]
        private int _sumValidWeights;

        [NonSerialized]
        private bool _cacheReady;

        public AudioClip Value
        {
            get
            {
                EnsureCache();

                if (entries == null || entries.Length == 0 || _sumValidWeights <= 0)
                    return null;

                int r = Random.Range(0, _sumValidWeights);
                int acc = 0;

                // Выбираем среди валидных
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e.weight <= 0 || e.value == null)
                        continue;

                    acc += e.weight;
                    if (r < acc)
                        return e.value;
                }

                // На случай рассинхрона/переполнения — возвращаем первый валидный
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e.weight > 0 && e.value != null)
                        return e.value;
                }

                return null;
            }
        }

        public float MaxLength
        {
            get
            {
                if (entries == null || entries.Length == 0)
                    return 0f;

                float max = 0f;
                for (int i = 0; i < entries.Length; i++)
                {
                    var c = entries[i].value;
                    if (c != null && c.length > max)
                        max = c.length;
                }
                return max;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            RebuildCache();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // ничего
        }

        private void EnsureCache()
        {
            if (!_cacheReady)
                RebuildCache();
        }

        private void RebuildCache()
        {
            _sumValidWeights = 0;

            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];

                    // Считаем только валидные
                    if (e.weight > 0 && e.value != null)
                    {
                        // защита от переполнения int (на всякий)
                        if (_sumValidWeights > int.MaxValue - e.weight)
                            _sumValidWeights = int.MaxValue;
                        else
                            _sumValidWeights += e.weight;
                    }
                }
            }

            _cacheReady = true;
        }
    }
}