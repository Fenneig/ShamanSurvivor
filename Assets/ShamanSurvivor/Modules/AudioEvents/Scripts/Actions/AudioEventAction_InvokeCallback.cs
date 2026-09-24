using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    public sealed class AudioEventAction_InvokeCallback : IAudioEventAction
    {
        [SerializeField]
        private AudioCallbackSerialized callback;
        
        public void Invoke(AudioEvent evt) => evt.InvokeCallback(this.callback);
    }
}