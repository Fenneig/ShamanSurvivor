using UnityEngine;

namespace Modules.AudioEvents
{
    public interface IClipSource : ISource<AudioClip>
    {
        float MaxLength { get; }
    }
}