using System;

namespace LetsPiu.Character.Uwb
{
    public interface IUwbTransport : IDisposable
    {
        bool IsRunning { get; }
        event Action<string> RawMessageReceived;
        void Start();
        void Stop();
        void Tick(float deltaTime);
    }
}
