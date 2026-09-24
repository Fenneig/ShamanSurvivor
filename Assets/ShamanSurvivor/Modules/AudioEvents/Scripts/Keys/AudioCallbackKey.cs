using System;

namespace Modules.AudioEvents
{
    public readonly struct AudioCallbackKey : IEquatable<AudioCallbackKey>
    {
        internal readonly int id;

        public AudioCallbackKey(string key) => this.id = AudioIdStore.NameToId(key);

        public AudioCallbackKey(int id) => this.id = id;

        public bool Equals(AudioCallbackKey other) => id == other.id;

        public override bool Equals(object obj) => obj is AudioCallbackKey other && Equals(other);

        public override int GetHashCode() => id;
    }
}