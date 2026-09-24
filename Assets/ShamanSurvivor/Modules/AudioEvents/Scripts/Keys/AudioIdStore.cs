using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.AudioEvents
{
    public static class AudioIdStore
    {
        private const int INITIAL_ID = 1;

        private static int _nextId = INITIAL_ID;

        private static readonly Dictionary<string, int> _nameToId = new();
        private static readonly Dictionary<int, string> _idToName = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NameToId(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (_nameToId.TryGetValue(name, out var id))
                return id;

            id = _nextId++;

            _nameToId[name] = id;
            _idToName[id] = name;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IdToName(int id)
        {
            return _idToName.TryGetValue(id, out string name)
                ? name
                : $"#Unknown:{id}";
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
#endif
        public static void Reset()
        {
            _nextId = INITIAL_ID;
            _nameToId.Clear();
            _idToName.Clear();
        }
    }
}