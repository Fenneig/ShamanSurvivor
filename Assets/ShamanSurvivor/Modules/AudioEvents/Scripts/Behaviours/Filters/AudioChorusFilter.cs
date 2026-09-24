using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.AudioEvents
{
    [Serializable]
    public sealed class AudioChorusFilter : IAudioEventBehaviour
    {
        [SerializeReference] private IFloatSource dryMix = new FloatConst(0.5f);
        [SerializeReference] private IFloatSource delay = new FloatConst(40);
        [SerializeReference] private IFloatSource rate = new FloatConst(0.8f);
        [SerializeReference] private IFloatSource depth = new FloatConst(0.03f);

        [Space] 
        [SerializeReference] private IFloatSource wetMix1 = new FloatConst(0.5f);
        [SerializeReference] private IFloatSource wetMix2 = new FloatConst(0.5f);
        [SerializeReference] private IFloatSource wetMix3 = new FloatConst(0.5f);

        private UnityEngine.AudioChorusFilter _filter;

        public void OnStart(AudioEvent evt)
        {
            _filter = evt.Source.gameObject.AddComponent<UnityEngine.AudioChorusFilter>();
            
            _filter.dryMix = this.dryMix?.Value ?? 0.5f;
            _filter.delay = this.delay?.Value ?? 40;
            _filter.rate = this.rate?.Value ?? 0.8f;
            _filter.depth = this.depth?.Value ?? 0.03f;

            _filter.wetMix1 = wetMix1?.Value ?? 0.5f;
            _filter.wetMix2 = wetMix2?.Value ?? 0.5f;
            _filter.wetMix3 = wetMix3?.Value ?? 0.5f;
        }

        public void OnStop(AudioEvent evt)
        {
            if (_filter != null)
            {
                Object.Destroy(_filter);
                _filter = null;
            }
        }

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
        }

        public void OnReset(AudioEvent evt)
        {
        }
    }
}