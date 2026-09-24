using System;
using System.Collections.Generic;
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
    public struct AudioParameterSerialized : ISerializationCallbackReceiver
    {
#if ODIN_INSPECTOR
        [HorizontalGroup]
        [HideLabel]
        [OnValueChanged(nameof(RebuildKey))]
#endif
        [SerializeField]
        private AudioBank _bank;

#if ODIN_INSPECTOR
        [HorizontalGroup]
        [HideLabel]
        [OnValueChanged(nameof(RebuildKey))]
        [ValueDropdown(nameof(ValuesDropdown))]
#endif
        [SerializeField]
        private string _parameter;

        [HideInInspector]
        [SerializeField]
        private string _key;

        public string Key
        {
            get
            {
                if (_key == null)
                    this.RebuildKey();

                return _key;
            }
        }

        public static implicit operator AudioParameterKey(AudioParameterSerialized it) => new(it.Key);

        void ISerializationCallbackReceiver.OnBeforeSerialize() =>
            this.RebuildKey();

        void ISerializationCallbackReceiver.OnAfterDeserialize() =>
            this.RebuildKey();

        private void RebuildKey()
        {
            _key = _bank == null || string.IsNullOrEmpty(_parameter)
                ? string.Empty
                : $"{_bank.Identifier}.{_parameter}";
        }

#if UNITY_EDITOR
        private IEnumerable<string> ValuesDropdown()
            => _bank == null ? Array.Empty<string>() : _bank.GetAllParameterNames();
#endif
    }
}