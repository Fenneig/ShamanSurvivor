namespace Modules.AudioEvents
{
    public interface IAudioEventBehaviour
    {
        void OnStart(AudioEvent evt);

        void OnUpdate(AudioEvent evt, float deltaTime);

        void OnStop(AudioEvent evt);

        void OnReset(AudioEvent evt);
    }
}