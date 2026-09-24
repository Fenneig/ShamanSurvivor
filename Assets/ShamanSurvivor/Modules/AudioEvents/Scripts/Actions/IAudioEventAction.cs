namespace Modules.AudioEvents
{
    public interface IAudioEventAction
    {
        void Invoke(AudioEvent evt);
    }
}