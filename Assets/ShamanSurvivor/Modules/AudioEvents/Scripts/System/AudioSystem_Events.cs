using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public sealed partial class AudioSystem
    {
        private static readonly AnimationCurve s_FadeOutLinearCurve = AnimationCurve.Linear(0, 1, 1, 0);

#if ODIN_INSPECTOR
        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif

        private readonly Dictionary<AudioEvent, int> _createdEvents = new();
        private readonly List<KeyValuePair<AudioEvent, int>> _createdEventsCache = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly List<AudioEvent> _playingEvents = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<int, float> _antiSpamEvents = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<AudioEvent, Transform> _eventParents = new();

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<AudioEvent, Coroutine> _fadeoutCoroutines = new();

        #region Play

#if ODIN_INSPECTOR
        [Title("Methods")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, float threshold = 0) =>
            this.PlayEvent(key, Vector3.zero, Quaternion.identity, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, out AudioEventHandle handle, float threshold = 0) =>
            this.PlayEvent(key, Vector3.zero, Quaternion.identity, out handle, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, Vector3 position, float threshold = 0) =>
            this.PlayEvent(key, position, Quaternion.identity, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, Vector3 position, out AudioEventHandle handle,
            float threshold = 0) =>
            this.PlayEvent(key, position, Quaternion.identity, out handle, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, Vector3 position, Quaternion rotation, float threshold = 0)
            => this.PlayEvent(key, position, rotation, out _, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(AudioEventKey key, Transform parent, float threshold = 0) =>
            this.PlayEvent(key, parent, out _, threshold);

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public bool PlayEvent(
            AudioEventKey key,
            Vector3 position,
            Quaternion rotation,
            out AudioEventHandle handle,
            float threshold = 0
        )
        {
            handle = default;

            if (this == null)
            {
                Debug.LogWarning("AudioSystem is null!");
                return false;
            }

            float now = Time.unscaledTime;
            int rawId = key.id;
            if (_antiSpamEvents.TryGetValue(rawId, out float nextAllowed) && now < nextAllowed)
                return false;

            if (!this.audioEventPool.TryRent(rawId, out AudioEvent audioEvent))
            {
                Debug.LogWarning($"AudioEvent with {key} is not found!");
                return false;
            }

            audioEvent.SetPositionAndRotation(position, rotation);
            audioEvent.SetCallback(this.DisposeEventInternal);

            _createdEvents.Add(audioEvent, rawId);

            if (threshold > 0)
                _antiSpamEvents[rawId] = now + threshold;

            this.StartEvent(audioEvent);
            handle = new AudioEventHandle(audioEvent, this);
            return true;
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public bool PlayEvent(AudioEventKey key, Transform parent, out AudioEventHandle handle, float threshold = 0)
        {
            handle = default;

            if (this == null)
            {
                Debug.LogWarning("AudioSystem is null!");
                return false;
            }

            float now = Time.unscaledTime;
            int rawId = key.id;
            if (_antiSpamEvents.TryGetValue(rawId, out float nextAllowed) && now < nextAllowed)
                return false;

            if (!this.audioEventPool.TryRent(rawId, out AudioEvent audioEvent))
            {
                Debug.LogWarning($"AudioEvent with {key} is not found!");
                return false;
            }

            audioEvent.SetParent(parent);
            audioEvent.SetCallback(this.DisposeEventInternal);

            _createdEvents.Add(audioEvent, rawId);
            _eventParents.Add(audioEvent, parent);

            if (threshold > 0)
                _antiSpamEvents[rawId] = now + threshold;

            this.StartEvent(audioEvent);
            handle = new AudioEventHandle(audioEvent, this);
            return true;
        }

        #endregion

        #region Create

        public AudioEventHandle CreateEvent(AudioEventKey key) =>
            this.CreateEvent(key, Vector3.zero, Quaternion.identity);

        public AudioEventHandle CreateEvent(AudioEventKey key, Vector3 position, Quaternion rotation)
        {
            AudioEvent audioEvent = this.audioEventPool.Rent(key.id);
            audioEvent.SetPositionAndRotation(position, rotation);
            audioEvent.SetCallback(null);

            _createdEvents.Add(audioEvent, key.id);
            return new AudioEventHandle(audioEvent, this);
        }

        public bool TryCreateEvent(AudioEventKey key, out AudioEventHandle result) =>
            this.TryCreateEvent(key, Vector3.zero, Quaternion.identity, out result);

        public bool TryCreateEvent(AudioEventKey key, Vector3 position, Quaternion rotation,
            out AudioEventHandle result)
        {
            if (!this.audioEventPool.TryRent(key.id, out AudioEvent audioEvent))
            {
                result = default;
                return false;
            }

            audioEvent.SetPositionAndRotation(position, rotation);
            audioEvent.SetCallback(null);

            _createdEvents.Add(audioEvent, key.id);

            result = new AudioEventHandle(audioEvent, this);
            return true;
        }

        internal bool IsCreatedEvent(AudioEvent audioEvent) =>
            _createdEvents.ContainsKey(audioEvent);

        #endregion

        #region Dispose

        public bool DisposeEvent(AudioEventKey key)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            int rawId = key.id;
            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEvent audioEvent, int otherId) = _createdEventsCache[i];
                if (otherId == rawId)
                {
                    this.DisposeEventInternal(audioEvent);
                    return true;
                }
            }

            return false;
        }

        public void DisposeEvents(AudioEventKey key)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            int rawId = key.id;
            
            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEvent audioEvent, int otherId) = _createdEventsCache[i];
                if (otherId == rawId)
                    this.DisposeEventInternal(audioEvent);
            }
        }

        public bool DisposeEvent(AudioEventKey key, float fadeoutTime) =>
            this.DisposeEvent(key, s_FadeOutLinearCurve, fadeoutTime);

        public void DisposeEvents(AudioEventKey key, float fadeoutTime) =>
            this.DisposeEvents(key, s_FadeOutLinearCurve, fadeoutTime);

        public bool DisposeEvent(AudioEventKey key, AnimationCurve curve, float fadeoutTime)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            int rawId = key.id;
            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEvent audioEvent, int otherId) = _createdEventsCache[i];
                if (otherId == rawId)
                    return DisposeEventInternal(audioEvent, curve, fadeoutTime);
            }

            return false;
        }

        private bool DisposeEventInternal(AudioEvent audioEvent, AnimationCurve curve, float fadeoutTime)
        {
            if (this == null)
                return false;

            this.StartCoroutine(this.DisposeEventRoutine(audioEvent, curve, fadeoutTime));
            return true;
        }

        internal void DisposeEventInternal(AudioEvent audioEvent)
        {
            if (!_createdEvents.Remove(audioEvent))
                return;

            if (_fadeoutCoroutines.Remove(audioEvent, out var c))
                StopCoroutine(c);

            if (_playingEvents.Remove(audioEvent))
                audioEvent.OnStop();

            if (_eventParents.Remove(audioEvent))
                audioEvent.SetParent(this.transform);

            this.audioEventPool.Return(audioEvent);
        }

        private IEnumerator DisposeEventRoutine(AudioEvent audioEvent, AnimationCurve curve, float fadeoutTime)
        {
            if (!_createdEvents.Remove(audioEvent))
                yield break;

            if (_playingEvents.Remove(audioEvent))
            {
                yield return audioEvent.FadeOut(curve, fadeoutTime);
                audioEvent.OnStop();
            }

            if (_eventParents.Remove(audioEvent))
                audioEvent.SetParent(this.transform);

            this.audioEventPool.Return(audioEvent);
        }

        public void DisposeEvents(AudioEventKey key, AnimationCurve curve, float fadeoutTime)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            int rawId = key.id;
            
            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEvent audioEvent, int otherId) = _createdEventsCache[i];
                if (otherId == rawId)
                    this.StartCoroutine(this.DisposeEventRoutine(audioEvent, curve, fadeoutTime));
            }
        }

        #endregion

        #region Find

        public bool FindAnyEvent(AudioEventKey key, out AudioEventHandle result)
        {
            int rawId = key.id;
            
            foreach ((AudioEvent audioEventBase, int otherId) in _createdEvents)
            {
                if (otherId == rawId)
                {
                    result = new AudioEventHandle(audioEventBase, this);
                    return true;
                }
            }

            result = default;
            return false;
        }

        public int FindAllEvents(AudioEventKey key, AudioEventHandle[] results)
        {
            int rawId = key.id;
            
            int count = 0;
            foreach ((AudioEvent audioEventBase, int otherId) in _createdEvents)
            {
                if (otherId != rawId) continue;
                if (count >= results.Length) break; // fix
                results[count++] = new AudioEventHandle(audioEventBase, this);
            }

            return count;
        }

        #endregion

        #region Lifecycle

        internal void StartEvent(AudioEvent audioEvent)
        {
            if (_fadeoutCoroutines.Remove(audioEvent, out Coroutine coroutine))
                this.StopCoroutine(coroutine);

            if (!_playingEvents.Contains(audioEvent))
            {
                audioEvent.OnStart();
                _playingEvents.Add(audioEvent);
            }
        }

        internal bool IsPlayingEvent(AudioEvent audioEvent)
        {
            return _playingEvents.Contains(audioEvent);
        }

        internal void StopEvent(AudioEvent audioEvent, float fadeoutTime)
        {
            this.StopEvent(audioEvent, fadeoutTime, s_FadeOutLinearCurve);
        }

        internal void StopEvent(AudioEvent audioEvent, float fadeoutTime, AnimationCurve curve)
        {
            bool valid = this != null;

            if (_fadeoutCoroutines.Remove(audioEvent, out Coroutine coroutine) && valid)
                this.StopCoroutine(coroutine);

            if (valid)
            {
                coroutine = this.StartCoroutine(this.StopEventRoutine(audioEvent, fadeoutTime, curve));
                _fadeoutCoroutines.Add(audioEvent, coroutine);
            }
        }

        private IEnumerator StopEventRoutine(AudioEvent audioEvent, float fadeoutTime, AnimationCurve curve)
        {
            yield return audioEvent.FadeOut(curve, fadeoutTime);
            if (_playingEvents.Remove(audioEvent))
                audioEvent.OnStop();

            _fadeoutCoroutines.Remove(audioEvent);
        }

        internal void StopEvent(AudioEvent audioEvent)
        {
            if (_playingEvents.Remove(audioEvent))
                audioEvent.OnStop();
        }

        #endregion

        private void UpdatePlayingEvents(float deltaTime)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _playingEvents.Count; i++)
                _playingEvents[i].OnUpdate(deltaTime);
        }
    }
}