using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    public sealed class AudioLowPassFilter : IAudioEventBehaviour
    {
        [SerializeReference]
        private IFloatSource cutoffFrequency = new FloatConst(5007.7f);
    
        [SerializeReference]
        private IFloatSource lowpassResonanceQ = new FloatConst(1);

        private UnityEngine.AudioLowPassFilter _filter;

        public void OnStart(AudioEvent evt)
        {
            _filter = evt.Source.gameObject.AddComponent<UnityEngine.AudioLowPassFilter>();
            _filter.cutoffFrequency = this.cutoffFrequency.Value;
            _filter.lowpassResonanceQ = this.lowpassResonanceQ.Value;
        }

        public void OnStop(AudioEvent evt)
        {
            if (_filter != null)
                GameObject.Destroy(_filter);
        }

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
        }

        public void OnReset(AudioEvent evt)
        {
        }
    }
}