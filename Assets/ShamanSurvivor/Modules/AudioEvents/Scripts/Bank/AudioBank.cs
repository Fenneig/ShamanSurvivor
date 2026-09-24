using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    [CreateAssetMenu(
        fileName = "AudioBank",
        menuName = "AudioEvents/New Audio Bank"
    )]
    public sealed partial class AudioBank : ScriptableObject
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
        
        [SerializeField]
        private string _identifier;

        [Space(8)]
        [SerializeField]
        private Entry<AudioEvent>[] _events;

        [Space(8)]
        [SerializeField]
        private Entry<bool>[] _boolParameters;

        [Space(8)]
        [SerializeField]
        private Entry<int>[] _intParameters;

        [Space(8)]
        [SerializeField]
        private Entry<float>[] _floatParameters;

        [Space(8)]
        [SerializeField]
        private string[] _callbacks;
        
        public string Identifier => _identifier;
        
        private void Reset()
        {
            _identifier = this.name;
        }

        public IReadOnlyList<Entry<AudioEvent>> Events => _events;
        public IReadOnlyList<Entry<bool>> BoolParameters => _boolParameters;
        public IReadOnlyList<Entry<int>> IntParameters => _intParameters;
        public IReadOnlyList<Entry<float>> FloatParameters => _floatParameters;
        public IReadOnlyList<string> Callbacks => _callbacks;

        public IEnumerable<string> GetAllParameterNames()
        {
            foreach (Entry<bool> parameter in _boolParameters)
                yield return parameter.name;

            foreach (Entry<int> parameter in _intParameters)
                yield return parameter.name;

            foreach (Entry<float> parameter in _floatParameters)
                yield return parameter.name;
        }

        public IEnumerable<string> GetAllEventNames() =>
            this.Events.Select(e => e.name);
    }
}