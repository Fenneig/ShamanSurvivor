using System;
using System.Collections.Generic;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public partial class AudioEvent
    {
#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<int, List<Action>> callbacks = new();

        public void AddCallback(AudioCallbackKey key, Action callback)
        {
            if (!this.callbacks.TryGetValue(key.id, out List<Action> callbacks))
            {
                callbacks = new List<Action>(1);
                this.callbacks.Add(key.id, callbacks);
            }

            callbacks.Add(callback);
        }

        public void RemoveCallback(AudioCallbackKey key, Action callback)
        {
            if (this.callbacks.TryGetValue(key.id, out List<Action> callbacks))
                callbacks.Remove(callback);
        }

        public void InvokeCallback(AudioCallbackKey key)
        {
            if (this.callbacks.TryGetValue(key.id, out List<Action> callbacks))
                for (int i = 0, count = callbacks.Count; i < count; i++)
                    callbacks[i].Invoke();
        }
    }
}