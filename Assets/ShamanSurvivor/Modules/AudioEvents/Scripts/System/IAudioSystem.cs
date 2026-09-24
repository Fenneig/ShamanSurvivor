using UnityEngine;

namespace Modules.AudioEvents
{
    public interface IAudioSystem
    {
        #region Lifecycle

        void Initialize();
        void Pause();
        void Resume();

        #endregion

        #region Banks

        bool RegisterBank(AudioBank bank);
        bool UnregisterBank(AudioBank bank);

        #endregion

        #region Events

        bool PlayEvent(AudioEventKey key, float threshold = 0);
        bool PlayEvent(AudioEventKey key, out AudioEventHandle handle, float threshold = 0);
        
        bool PlayEvent(AudioEventKey key, Vector3 position, float threshold = 0);
        bool PlayEvent(AudioEventKey key, Vector3 position, out AudioEventHandle handle, float threshold = 0);
        bool PlayEvent(AudioEventKey key, Vector3 position, Quaternion rotation, float threshold = 0);
        bool PlayEvent(AudioEventKey key, Vector3 position, Quaternion rotation, out AudioEventHandle handle, float threshold = 0);
        bool PlayEvent(AudioEventKey key, Transform parent, float threshold = 0);
        bool PlayEvent(AudioEventKey key, Transform parent, out AudioEventHandle handle, float threshold = 0);

        bool TryCreateEvent(AudioEventKey key, out AudioEventHandle result);
        bool TryCreateEvent(AudioEventKey key, Vector3 position, Quaternion rotation, out AudioEventHandle result);
        AudioEventHandle CreateEvent(AudioEventKey key);
        AudioEventHandle CreateEvent(AudioEventKey key, Vector3 position, Quaternion rotation);
      
        bool DisposeEvent(AudioEventKey key);
        bool DisposeEvent(AudioEventKey key, float fadeoutTime);
        bool DisposeEvent(AudioEventKey key, AnimationCurve curve, float fadeoutTime);
        
        void DisposeEvents(AudioEventKey key);
        void DisposeEvents(AudioEventKey key, float fadeoutTime);
        void DisposeEvents(AudioEventKey key, AnimationCurve curve, float fadeoutTime);
        
        bool FindAnyEvent(AudioEventKey key, out AudioEventHandle result);
        int FindAllEvents(AudioEventKey key, AudioEventHandle[] results);

        #endregion

        #region Parameters

        bool TryGetBool(AudioParameterKey key, out bool result);
        bool TryGetInt(AudioParameterKey key, out int result);
        bool TryGetFloat(AudioParameterKey key, out float result);
        
        bool GetBool(AudioParameterKey key);
        int GetInt(AudioParameterKey key);
        float GetFloat(AudioParameterKey key);
        
        void SetBool(AudioParameterKey key, bool value);
        void SetInt(AudioParameterKey key, int value);
        void SetFloat(AudioParameterKey key, float value);

        #endregion
    }
}