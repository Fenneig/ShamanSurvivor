using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    internal sealed class AudioEventBehaviour_VolumeCurve : IAudioEventBehaviour
    {
        [SerializeField]
        private AnimationCurve curve;

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
            evt.Source.volume = this.curve.Evaluate(evt.CurrentProgress);
        }

        public void OnReset(AudioEvent evt)
        {
            evt.Source.volume = this.curve.Evaluate(evt.CurrentProgress);
        }

        public void OnStart(AudioEvent evt)
        {
        }

        public void OnStop(AudioEvent evt)
        {
        }
    }
}