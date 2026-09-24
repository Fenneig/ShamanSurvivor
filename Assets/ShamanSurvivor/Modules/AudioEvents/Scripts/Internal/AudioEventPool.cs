using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    internal sealed class AudioEventPool : IDisposable
    {
        private readonly AudioSystem _audioSystem;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private readonly Dictionary<int, (AudioEvent, Stack<AudioEvent>)> _pool = new();

        internal AudioEventPool(AudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
        }

        internal void Register(int identifier, AudioEvent prefab)
        {
            Stack<AudioEvent> stack = prefab.Poolable ? new Stack<AudioEvent>() : null;
            _pool[identifier] = (prefab, stack);
        }

        internal void Unregister(int identifier)
        {
            if (!_pool.Remove(identifier, out (AudioEvent prefab, Stack<AudioEvent> stack) tuple))
                return;

            if (!tuple.prefab.Poolable)
                return;

            foreach (var audioEvent in tuple.stack)
                ScriptableObject.Destroy(audioEvent);

            tuple.stack.Clear();
        }

        internal AudioEvent Rent(int identifier)
        {
            if (!_pool.TryGetValue(identifier, out (AudioEvent prefab, Stack<AudioEvent> stack) tuple))
                throw new Exception($"Event id {identifier} is not found!");

            if (!tuple.prefab.Poolable || !tuple.stack.TryPop(out AudioEvent evt))
            {
                evt = ScriptableObject.Instantiate(tuple.prefab);
                evt.OnCreate(identifier, _audioSystem);
            }

            evt.OnSpawn();
            return evt;
        }

        internal bool TryRent(int identifier, out AudioEvent evt)
        {
            if (!_pool.TryGetValue(identifier, out (AudioEvent prefab, Stack<AudioEvent> stack) tuple))
            {
                evt = null;
                return false;
            }

            if (!tuple.prefab.Poolable || !tuple.stack.TryPop(out evt))
            {
                evt = ScriptableObject.Instantiate(tuple.prefab);
                evt.OnCreate(identifier, _audioSystem);
            }

            evt.OnSpawn();
            return true;
        }

        internal void Return(AudioEvent audioEvent)
        {
            int identifier = audioEvent.Id;
            audioEvent.OnDespawn();

            if (_pool.TryGetValue(identifier, out (AudioEvent prefab, Stack<AudioEvent> stack) tuple))
            {
                if (tuple.prefab.Poolable)
                    tuple.stack.Push(audioEvent);
                else
                    ScriptableObject.Destroy(audioEvent);
            }
        }

        public void Dispose()
        {
            foreach ((AudioEvent prefab, Stack<AudioEvent> stack) in _pool.Values)
            {
                if (!prefab.Poolable)
                    continue;

                foreach (AudioEvent audioEvent in stack)
                    ScriptableObject.Destroy(audioEvent);

                stack.Clear();
            }

            _pool.Clear();
        }
    }
}