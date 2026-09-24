using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    [Serializable]
    public sealed class AudioEventAction_PlayAudioEvent : IAudioEventAction
    {
        [SerializeField]
        private AudioEventSerialized eventId;

        [SerializeField]
        private float _threshold;

        public void Invoke(AudioEvent evt)
        {
            evt.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
            evt.System.PlayEvent(this.eventId, position, rotation, _threshold);
        }
    }
}