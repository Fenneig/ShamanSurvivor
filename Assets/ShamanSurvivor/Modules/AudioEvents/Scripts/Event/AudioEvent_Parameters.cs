using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public partial class AudioEvent
    {
#if ODIN_INSPECTOR
        [InlineProperty]
#endif
        [Serializable]
        public struct Entry<T>
        {
#if ODIN_INSPECTOR
            [HorizontalGroup]
#endif
            [SerializeField]
            public string name;

#if ODIN_INSPECTOR
            [HorizontalGroup]
#endif
            [SerializeField]
            public T value;
        }

#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly List<Entry<bool>> _boolParameters = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly List<Entry<int>> _intParameters = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly List<Entry<float>> _floatParameters = new();

        public bool TryGetBool(AudioParameterKey key, out bool result)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (Entry<bool> entry in _boolParameters)
            {
                if (entry.name == name)
                {
                    result = entry.value;
                    return true;
                }
            }

            result = false;
            return false;
        }

        public bool TryGetInt(AudioParameterKey key, out int result)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (Entry<int> entry in _intParameters)
            {
                if (entry.name == name)
                {
                    result = entry.value;
                    return true;
                }
            }

            result = 0;
            return false;
        }

        public bool TryGetFloat(AudioParameterKey key, out float result)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (Entry<float> entry in _floatParameters)
            {
                if (entry.name == name)
                {
                    result = entry.value;
                    return true;
                }
            }

            result = 0;
            return false;
        }

        public bool GetBool(AudioParameterKey key) => this.TryGetBool(key, out bool value)
            ? value
            : throw new KeyNotFoundException(AudioIdStore.IdToName(key.id));

        public int GetInt(AudioParameterKey key) => this.TryGetInt(key, out var value)
            ? value
            : throw new KeyNotFoundException(AudioIdStore.IdToName(key.id));

        public float GetFloat(AudioParameterKey key) => this.TryGetFloat(key, out float value)
            ? value
            : throw new KeyNotFoundException(AudioIdStore.IdToName(key.id));

        public bool HasBool(AudioParameterKey key)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (Entry<bool> entry in _boolParameters)
                if (entry.name == name)
                    return true;

            return false;
        }

        public bool HasInt(AudioParameterKey key)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (Entry<int> entry in _intParameters)
                if (entry.name == name)
                    return true;

            return false;
        }

        public bool HasFloat(AudioParameterKey key)
        {
            string name = AudioIdStore.IdToName(key.id);
            foreach (var entry in _floatParameters)
                if (entry.name == name)
                    return true;

            return false;
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetBool(AudioParameterKey key, bool value)
        {
            string name = AudioIdStore.IdToName(key.id);
            for (int i = 0; i < _boolParameters.Count; i++)
            {
                if (_boolParameters[i].name == name)
                {
                    _boolParameters[i] = new Entry<bool> {name = name, value = value};
                    return;
                }
            }

            _boolParameters.Add(new Entry<bool> {name = name, value = value});
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetInt(AudioParameterKey key, int value)
        {
            string name = AudioIdStore.IdToName(key.id);
            for (int i = 0; i < _intParameters.Count; i++)
            {
                if (_intParameters[i].name == name)
                {
                    _intParameters[i] = new Entry<int> {name = name, value = value};
                    return;
                }
            }

            _intParameters.Add(new Entry<int> {name = name, value = value});
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetFloat(AudioParameterKey key, float value)
        {
            string name = AudioIdStore.IdToName(key.id);
            for (int i = 0; i < _floatParameters.Count; i++)
            {
                if (_floatParameters[i].name == name)
                {
                    _floatParameters[i] = new Entry<float> {name = name, value = value};
                    return;
                }
            }

            _floatParameters.Add(new Entry<float> {name = name, value = value});
        }
    }
}