using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    internal sealed class AudioEventBehaviour_VolumeParameter : IAudioEventBehaviour
    {
        [SerializeField]
        private AudioParameterSerialized _volume;

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
            if (evt.System.TryGetFloat(_volume, out float volume))
                evt.Source.volume = Mathf.Clamp01(volume);
        }

        public void OnReset(AudioEvent evt)
        {
            if (evt.System.TryGetFloat(_volume, out float volume))
                evt.Source.volume = Mathf.Clamp01(volume);
        }

        public void OnStart(AudioEvent evt)
        {
        }

        public void OnStop(AudioEvent evt)
        {
        }
    }
}