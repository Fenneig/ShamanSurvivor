using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnassignedField.Local

namespace Modules.AudioEvents
{
    [CreateAssetMenu(
        fileName = "AudioEvent",
        menuName = "AudioEvents/New Audio Event"
    )]
    public sealed partial class AudioEvent : ScriptableObject
    {
        [Serializable]
        private sealed class BehaviourEntry
        {
            [SerializeReference]
            public IAudioEventBehaviour value;

            [SerializeField, Space]
            public bool mute;

#if ODIN_INSPECTOR
            [DisableIf(nameof(mute))]
#endif
            [SerializeField]
            public bool fullTime = true;

#if ODIN_INSPECTOR
            [DisableIf(nameof(mute))]
            [HideIf(nameof(fullTime))]
#endif
            [SerializeField]
            public float startTime;

#if ODIN_INSPECTOR
            [DisableIf(nameof(mute))]
            [HideIf(nameof(fullTime))]
            [SerializeField]
#endif
            public float endTime;
        }

        [Serializable]
        private sealed class ActionEntry
        {
            [SerializeReference]
            public IAudioEventAction value;

            [SerializeField, Space]
            public bool mute;

#if ODIN_INSPECTOR
            [DisableIf(nameof(mute))]
#endif
            [SerializeField]
            public float time;

            internal bool passed;
        }

#if ODIN_INSPECTOR
        [HideInPlayMode]
#endif
        [SerializeField]
        private bool _poolable = true;

        [Space]
        [SerializeField]
        private AudioMixerGroup output;

        [SerializeField]
        private bool loop;

        [SerializeReference]
        private IFloatSource startTime = new FloatConst();

#if ODIN_INSPECTOR
        [EnableIf(nameof(overrideDuration))]
#endif
        [Space]
        [SerializeField]
        private float duration;

        [SerializeField]
        private bool overrideDuration;

#if ODIN_INSPECTOR
        [SuffixLabel("2D — 3D")]
#endif
        [Space]
        [SerializeField, Range(0, 1)]
        private float spatialBlend;

#if ODIN_INSPECTOR
        [Title("Audio")]
#endif
        [SerializeReference]
        private IClipSource clip = new ClipConst();

        [SerializeReference]
        private IFloatSource pitch = new FloatConst(1);

        [SerializeReference]
        private IFloatSource volume = new FloatConst(1);

        [SerializeReference]
        private IFloatSource reverbZoneMix = new FloatConst(1);

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
#endif
        [SerializeField, Space]
        private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
        [ShowIf(nameof(rolloffMode), AudioRolloffMode.Custom)]
#endif
        [SerializeField]
        private AnimationCurve rolloffCurve;

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
#endif
        [SerializeReference, Space]
        private IFloatSource minDistance = new FloatConst(1);

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
#endif
        [SerializeReference]
        private IFloatSource maxDistance = new FloatConst(500);

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
#endif
        [SerializeReference]
        private IFloatSource dopplerLevel = new FloatConst(0.8f);

#if ODIN_INSPECTOR
        [FoldoutGroup("3D")]
#endif
        [SerializeReference]
        private IFloatSource spread = new FloatConst(0);

        [Header("Behaviour")]
        [SerializeField]
        private BehaviourEntry[] behaviours;

        [SerializeField, Space]
        private ActionEntry[] actions;

        private int _identifier;
        private AudioSystem _audioSystem;

        private bool _spawned;
        private AudioSource _audioSource;

        private float _currentTime;
        private float _currentProgress;
        private Action<AudioEvent> _completeCallback;

        public int Id => _identifier;
        public bool Poolable => _poolable;
        public bool IsSpawned => _spawned;

        public AudioSystem System => _audioSystem;
        public AudioSource Source => _audioSource;

        public float CurrentTime => _currentTime;
        public float CurrentProgress => _currentProgress;

        internal void SetCallback(Action<AudioEvent> callback) => _completeCallback = callback;

        internal void OnCreate(int identifier, AudioSystem audioSystem)
        {
            _identifier = identifier;
            _audioSystem = audioSystem;
        }

        internal void OnSpawn()
        {
            if (_spawned)
            {
                Debug.LogWarning($"AudioEvent {AudioIdStore.IdToName(_identifier)} is already spawned", this);
                return;
            }

            if (_audioSystem == null)
            {
                Debug.LogWarning($"AudioEvent {AudioIdStore.IdToName(_identifier)}: AudioSystem is null", this);
                return;
            }

            _audioSource = _audioSystem.RentAudioSource();
            _audioSource.name = AudioIdStore.IdToName(_identifier);
            _audioSource.outputAudioMixerGroup = output;
            _audioSource.loop = loop;
            _audioSource.spatialBlend = spatialBlend;

            _spawned = true;
        }

        internal void OnDespawn()
        {
            if (!_spawned)
            {
                Debug.LogWarning($"AudioEvent {_identifier} not spawned yet", this);
                return;
            }

            // На всякий случай
            if (_audioSource != null)
                _audioSource.Stop();

            _audioSystem.ReturnAudioSource(_audioSource);
            _audioSource = null;

            _spawned = false;
        }

        #region Start/Stop/Pause

        internal void OnStart()
        {
            if (!_spawned || _audioSource == null)
            {
                Debug.LogWarning($"AudioEvent {_identifier}: cannot start, not spawned or AudioSource is null", this);
                return;
            }

            ResetState();

            // Если клипа нет — завершаем сразу (или можно просто return без коллбека — по желанию)
            if (_audioSource.clip == null)
            {
                Debug.LogWarning($"AudioEvent {_identifier}: clip is null", this);
                _completeCallback?.Invoke(this);
                return;
            }

            _audioSource.Play();
            StartBehaviours();
        }

        internal void OnStop()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            StopBehaviours();
        }

        internal void OnPause()
        {
            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.Pause();
        }

        internal void OnResume()
        {
            if (_audioSource != null && !_audioSource.isPlaying)
                _audioSource.UnPause();
        }

        #endregion

        #region Update

        internal void OnUpdate(float deltaTime)
        {
            if (!_spawned || _audioSource == null)
                return;

            float effectiveDuration = GetEffectiveDuration();

            // Если длительности нет, мы не можем корректно считать прогресс
            if (effectiveDuration <= 0f)
                return;

            StartUpdate(deltaTime, effectiveDuration);
            ProcessUpdate(deltaTime);
            FinishUpdate();
        }

        private void StartUpdate(float deltaTime, float effectiveDuration)
        {
            _currentTime = Mathf.Min(_currentTime + deltaTime, effectiveDuration);
            _currentProgress = Mathf.Clamp01(_currentTime / effectiveDuration);
        }

        private void ProcessUpdate(float deltaTime)
        {
            // Actions
            if (actions != null)
            {
                for (int i = 0, count = actions.Length; i < count; i++)
                {
                    ActionEntry action = actions[i];
                    if (action == null || action.mute || action.value == null)
                        continue;

                    if (!action.passed && _currentTime >= action.time)
                    {
                        action.value.Invoke(this);
                        action.passed = true;
                    }
                }
            }

            // Behaviours
            if (behaviours != null)
            {
                for (int i = 0, count = behaviours.Length; i < count; i++)
                {
                    BehaviourEntry behaviour = behaviours[i];
                    if (behaviour == null || behaviour.mute || behaviour.value == null)
                        continue;

                    if (behaviour.fullTime ||
                        (_currentTime >= behaviour.startTime && _currentTime <= behaviour.endTime))
                        behaviour.value.OnUpdate(this, deltaTime);
                }
            }
        }

        private void FinishUpdate()
        {
            if (_currentProgress < 1f)
                return;

            if (loop)
            {
                ResetState();
                // Важно: если source.loop = true, AudioSource сам зациклится без остановки.
                // А ResetState меняет clip/time/etc. — это ок, если ты хочешь "логический луп" для actions/behaviours.
            }
            else
            {
                _completeCallback?.Invoke(this);
            }
        }

        #endregion

        #region FadeOut

        internal IEnumerator FadeOut(AnimationCurve curve, float fadeDuration)
        {
            if (_audioSource == null)
                yield break;

            if (fadeDuration <= 0f)
            {
                _audioSource.volume = 0f;
                yield break;
            }

            float time = 0f;
            float startVolume = _audioSource.volume;

            while (time < fadeDuration)
            {
                float t = time / fadeDuration;
                float fadeMultiplier = curve?.Evaluate(t) ?? (1f - t);

                _audioSource.volume = startVolume * fadeMultiplier;

                time += Time.unscaledDeltaTime; // чтобы фейд работал при timeScale = 0
                yield return null;
            }

            _audioSource.volume = 0f;
        }

        #endregion

        #region Behaviours helpers

        private void StartBehaviours()
        {
            if (behaviours == null) return;

            for (int i = 0, count = behaviours.Length; i < count; i++)
            {
                BehaviourEntry behaviour = behaviours[i];
                if (behaviour == null || behaviour.mute || behaviour.value == null)
                    continue;

                behaviour.value.OnStart(this);
            }
        }

        private void StopBehaviours()
        {
            if (behaviours == null) return;

            for (int i = 0, count = behaviours.Length; i < count; i++)
            {
                BehaviourEntry behaviour = behaviours[i];
                if (behaviour == null || behaviour.mute || behaviour.value == null)
                    continue;

                behaviour.value.OnStop(this);
            }
        }

        #endregion

        #region Reset

        private void ResetState()
        {
            if (_audioSource == null)
                return;

            _audioSource.volume = volume?.Value ?? 1f;
            _audioSource.pitch = pitch?.Value ?? 1f;
            _audioSource.clip = clip?.Value;
            _audioSource.reverbZoneMix = reverbZoneMix?.Value ?? 1f;

            // 3D
            _audioSource.dopplerLevel = dopplerLevel?.Value ?? _audioSource.dopplerLevel;
            _audioSource.spread = spread?.Value ?? _audioSource.spread;
            _audioSource.minDistance = minDistance?.Value ?? _audioSource.minDistance;
            _audioSource.maxDistance = maxDistance?.Value ?? _audioSource.maxDistance;
            _audioSource.rolloffMode = rolloffMode;

            if (rolloffMode == AudioRolloffMode.Custom && rolloffCurve != null)
                _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffCurve);

            // Time
            float st = startTime?.Value ?? 0f;
            st = _audioSource.clip != null ? Mathf.Clamp(st, 0f, _audioSource.clip.length) : Mathf.Max(0f, st);

            // Важно: time можно выставлять ДО Play
            _audioSource.time = st;

            _currentTime = 0f;
            _currentProgress = 0f;

            ResetActions();
            ResetBehaviours();
        }

        private void ResetBehaviours()
        {
            if (behaviours == null) return;

            for (int i = 0, count = behaviours.Length; i < count; i++)
            {
                BehaviourEntry behaviour = behaviours[i];
                if (behaviour == null || behaviour.mute || behaviour.value == null)
                    continue;

                behaviour.value.OnReset(this);
            }
        }

        private void ResetActions()
        {
            if (actions == null) return;

            for (int i = 0, count = actions.Length; i < count; i++)
            {
                ActionEntry action = actions[i];
                if (action == null) continue;
                action.passed = false;
            }
        }

        #endregion

        private float GetEffectiveDuration()
        {
            if (overrideDuration)
                return Mathf.Max(0f, duration);

            // если overrideDuration выключен — берём длину клипа
            AudioClip c = _audioSource != null ? _audioSource.clip : null;
            return c != null ? Mathf.Max(0f, c.length) : 0f;
        }
    }
}