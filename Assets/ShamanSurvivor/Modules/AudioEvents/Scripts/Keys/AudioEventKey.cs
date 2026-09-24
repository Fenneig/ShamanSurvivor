using System;

namespace Modules.AudioEvents
{
    public readonly struct AudioEventKey : IEquatable<AudioEventKey>
    {
        internal readonly int id;

        public AudioEventKey(string key) => this.id = AudioIdStore.NameToId(key);
        
        public AudioEventKey(int id) => this.id = id;

        public bool Equals(AudioEventKey other) => id == other.id;

        public override bool Equals(object obj) => obj is AudioEventKey other && Equals(other);

        public override int GetHashCode() => id;
    }
}