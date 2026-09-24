using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    internal sealed class AudioEventBehaviour_PitchCurve : IAudioEventBehaviour
    {
        [SerializeField]
        private AnimationCurve curve;

        public void OnStart(AudioEvent evt)
        {
        }

        public void OnStop(AudioEvent evt)
        {
        }

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
            evt.Source.pitch = this.curve.Evaluate(evt.CurrentProgress);
        }

        public void OnReset(AudioEvent evt)
        {
            evt.Source.pitch = this.curve.Evaluate(evt.CurrentProgress);
        }
    }
}