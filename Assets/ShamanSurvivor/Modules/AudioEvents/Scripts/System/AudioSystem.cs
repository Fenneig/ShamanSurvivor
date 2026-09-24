using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace Modules.AudioEvents
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Audio/Audio System", -100)]
    public sealed partial class AudioSystem : MonoBehaviour, IAudioSystem
    {
        [SerializeField]
        private bool initOnAwake = true;

#if ODIN_INSPECTOR
        [FoldoutGroup("Pools")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private AudioSourcePool audioSourcePool;

#if ODIN_INSPECTOR
        [FoldoutGroup("Pools")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private AudioEventPool audioEventPool;

        private bool isInited;

        public void Initialize()
        {
            if (isInited)
                return;
            
            this.audioSourcePool = new AudioSourcePool(this.transform);
            this.audioEventPool = new AudioEventPool(this);
            this.RegisterInitialBanks();

            isInited = true;
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void Pause()
        {
            this.enabled = false;

            foreach (AudioEvent playingEvent in _playingEvents) 
                playingEvent.OnPause();
        }

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void Resume()
        {
            this.enabled = true;

            foreach (AudioEvent playingEvent in _playingEvents) 
                playingEvent.OnResume();
        }

        internal AudioSource RentAudioSource() => this.audioSourcePool.Rent();

        internal void ReturnAudioSource(AudioSource audioSource) => this.audioSourcePool.Return(audioSource);

        private void Awake()
        {
            if (this.initOnAwake) 
                this.Initialize();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            this.UpdatePlayingEvents(deltaTime);
        }

        private void OnDestroy()
        {
            this.audioSourcePool?.Dispose();
            this.audioEventPool?.Dispose();
        }
    }
}