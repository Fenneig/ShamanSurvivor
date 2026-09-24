#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public sealed partial class AudioEvent
    {
        private static double _previewEndTime;

        private bool PreviewPlaying =>
            Math.Abs(_previewEndTime - -1) < float.Epsilon || EditorApplication.timeSinceStartup < _previewEndTime;

#if ODIN_INSPECTOR
        [GUIColor(1f, 0.83f, 0f)]
        [HorizontalGroup("Debug")]
        [DisableIf(nameof(PreviewPlaying))]
        [Button("Play")]
#endif
        private void PreviewPlay()
        {
            this.PreviewStop();

#if UNITY_EDITOR
            AudioClip audioClip = clip?.Value;
            if (audioClip == null)
                return;

            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            var playMethod = audioUtilType.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] {typeof(AudioClip), typeof(int), typeof(bool)},
                null);

            if (playMethod == null)
                return;

            int startSample = Mathf.RoundToInt(startTime.Value * audioClip.frequency);

            playMethod.Invoke(null, new object[] {audioClip, startSample, loop});

            if (loop)
            {
                _previewEndTime = -1; // бесконечное воспроизведение
            }
            else
            {
                double duration = audioClip.length - startTime.Value;
                _previewEndTime = EditorApplication.timeSinceStartup + duration;
            }
#endif
        }

#if ODIN_INSPECTOR
        [HorizontalGroup("Debug")]
        [EnableIf(nameof(PreviewPlaying))]
        [Button("Stop")]
#endif
        private void PreviewStop()
        {
#if UNITY_EDITOR
            Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            MethodInfo stopMethod = audioUtilType.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public);

            stopMethod?.Invoke(null, null);
            _previewEndTime = 0; // сброс
#endif
        }

        private void OnValidate()
        {
            if (!this.overrideDuration)
            {
                this.duration = this.clip?.MaxLength ?? 0;
                EditorUtility.SetDirty(this);
            }
        }
    }
}
#endif