using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    internal sealed class AudioEventBehaviour_PitchParameter : IAudioEventBehaviour
    {
        [SerializeField]
        private AudioParameterSerialized pitch;

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
            if (evt.System.TryGetFloat(this.pitch, out float pitch))
                evt.Source.pitch = pitch;
        }

        public void OnReset(AudioEvent evt)
        {
            if (evt.System.TryGetFloat(this.pitch, out float pitch))
                evt.Source.pitch = pitch;
        }

        public void OnStart(AudioEvent evt)
        {
        }

        public void OnStop(AudioEvent evt)
        {
        }
    }
}