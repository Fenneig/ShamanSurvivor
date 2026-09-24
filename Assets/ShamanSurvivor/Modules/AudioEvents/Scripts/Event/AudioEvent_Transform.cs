using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    public sealed partial class AudioEvent
    {
        public void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            if (_audioSource)
            {
                _audioSource.transform.GetPositionAndRotation(out position, out rotation);
            }
            else
            {
                position = default;
                rotation = default;
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (_audioSource)
                _audioSource.transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_audioSource)
                _audioSource.transform.rotation = rotation;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (_audioSource)
                _audioSource.transform.SetPositionAndRotation(position, rotation);
        }

        public void SetParent(Transform parent, bool withPositionAndRotation = true)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (!_audioSource)
                return;

            Transform audioTransform = _audioSource.transform;

            if (withPositionAndRotation)
            {
                parent.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                audioTransform.SetPositionAndRotation(position, rotation);
            }

            audioTransform.SetParent(parent);
        }
    }
}