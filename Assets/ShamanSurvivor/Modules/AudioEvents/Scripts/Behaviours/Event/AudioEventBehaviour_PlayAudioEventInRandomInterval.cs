using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Modules.AudioEvents
{
    [Serializable]
    public sealed class AudioEventBehaviour_PlayAudioEventInRandomInterval : IAudioEventBehaviour
    {
        [SerializeField]
        private float _startTime;

        [SerializeField]
        private float _endTime;

        [SerializeField]
        private AudioEventSerialized _event;

        private float _actionTimestamp;
        private bool _wasAction;

        public void OnStart(AudioEvent evt)
        {
        }

        public void OnStop(AudioEvent evt)
        {
        }

        public void OnUpdate(AudioEvent evt, float deltaTime)
        {
            if (_wasAction || evt.CurrentTime < _actionTimestamp)
                return;

            evt.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
            evt.System.PlayEvent(this._event, position, rotation);
            _wasAction = true;
        }

        public void OnReset(AudioEvent evt)
        {
            _actionTimestamp = Random.Range(_startTime, _endTime);
            _wasAction = false;
        }
    }
}