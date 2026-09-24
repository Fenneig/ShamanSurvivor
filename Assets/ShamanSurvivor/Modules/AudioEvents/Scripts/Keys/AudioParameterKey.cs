using System;

namespace Modules.AudioEvents
{
    public readonly struct AudioParameterKey : IEquatable<AudioParameterKey>
    {
        internal readonly int id;

        public AudioParameterKey(string key) => this.id = AudioIdStore.NameToId(key);

        public AudioParameterKey(int id) => this.id = id;

        public bool Equals(AudioParameterKey other) => id == other.id;

        public override bool Equals(object obj) => obj is AudioEventKey other && Equals(other);

        public override int GetHashCode() => id;
    }
}