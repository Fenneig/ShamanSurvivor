using System;
using System.Collections.Generic;
using System.Linq;
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
    public struct AudioEventSerialized : ISerializationCallbackReceiver
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
        [ValueDropdown(nameof(ValuesDropdown))]
        [OnValueChanged(nameof(RebuildKey))]
#endif
        [SerializeField]
        private string _event;

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

        public static implicit operator AudioEventKey(AudioEventSerialized it) => new(it.Key);

        void ISerializationCallbackReceiver.OnBeforeSerialize() =>
            this.RebuildKey();

        void ISerializationCallbackReceiver.OnAfterDeserialize() => 
            this.RebuildKey();

        private void RebuildKey() => 
            _key = _bank == null || string.IsNullOrEmpty(_event) ? string.Empty : $"{_bank.Identifier}.{_event}";

#if UNITY_EDITOR
        private IEnumerable<string> ValuesDropdown()
            => _bank == null ? Enumerable.Empty<string>() : _bank.GetAllEventNames();
#endif
    }
}