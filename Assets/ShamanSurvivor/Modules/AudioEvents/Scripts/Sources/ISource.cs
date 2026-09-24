namespace Modules.AudioEvents
{
    public interface ISource<out T>
    {
        T Value { get; }
    }
}