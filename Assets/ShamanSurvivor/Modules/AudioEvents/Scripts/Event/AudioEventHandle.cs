using System;
using UnityEngine;

namespace Modules.AudioEvents
{
    public readonly struct AudioEventHandle
    {
        private readonly AudioEvent _event;
        private readonly AudioSystem _system;

        public int Id => _event.Id;

        public bool IsPlaying => _system.IsPlayingEvent(_event);

        public bool IsValid =>
            _event != null && 
            _event.IsSpawned &&
            _system.IsCreatedEvent(_event);

        internal AudioEventHandle(AudioEvent @event, AudioSystem system)
        {
            _event = @event;
            _system = system;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (this.IsValid) 
                _event.SetPositionAndRotation(position, rotation);
        }

        public void SetPosition(Vector3 position)
        {
            if (this.IsValid) 
                _event.SetPosition(position);
        }

        public void SetRotation(Quaternion rotation)
        {
            if (this.IsValid) 
                _event.SetRotation(rotation);
        }
        
        public void Play(Vector3 position, Quaternion rotation)
        {
            if (this.IsValid)
            {
                _event.SetPositionAndRotation(position, rotation);
                _system.StartEvent(_event);
            }
        }

        public void Play()
        {
            if (this.IsValid)
                _system.StartEvent(_event);
        }

        public void Stop()
        {
            if (this.IsValid)
                _system.StopEvent(_event);
        }

        public void Stop(float fadeoutTime)
        {
            if (this.IsValid) 
                _system.StopEvent(_event, fadeoutTime);
        }

        public void Stop(float fadeoutTime, AnimationCurve curve)
        {
            if (this.IsValid) 
                _system.StopEvent(_event, fadeoutTime, curve);
        }

        public void Dispose()
        {
            if (this.IsValid)
                _system.DisposeEventInternal(_event);
        }
        
        public void Dispose(float fadeoutTime)
        {
            if (this.IsValid)
                _system.DisposeEvent(new AudioEventKey(_event.Id), fadeoutTime);
        }
        
        public void Dispose(float fadeoutTime, AnimationCurve curve)
        {
            if (this.IsValid)
                _system.DisposeEvent(new AudioEventKey(_event.Id), curve, fadeoutTime);
        }

        public void SetBool(AudioParameterKey key, bool value)
        {
            if (this.IsValid) 
                _event.SetBool(key, value);
        }
        
        public void SetInt(AudioParameterKey key, int value)
        {
            if (this.IsValid) 
                _event.SetInt(key, value);  
        }
        
        public void SetFloat(AudioParameterKey key, float value)
        {
            if (this.IsValid) 
                _event.SetFloat(key, value);
        }
        
        public void AddCallback(AudioCallbackKey key, Action callback)
        {
            if (this.IsValid) 
                _event.AddCallback(key, callback);
        }
        
        public void RemoveCallback(AudioCallbackKey key, Action callback)
        {
            if (this.IsValid) 
                _event.RemoveCallback(key, callback);
        }
    }
}