using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public partial class AudioSystem
    {
#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<int, bool> boolParameters = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<int, int> intParameters = new();

#if ODIN_INSPECTOR
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        private readonly Dictionary<int, float> floatParameters = new();

        public bool TryGetBool(AudioParameterKey paramId, out bool result) =>
            this.boolParameters.TryGetValue(paramId.id, out result);

        public bool TryGetInt(AudioParameterKey paramId, out int result) =>
            this.intParameters.TryGetValue(paramId.id, out result);

        public bool TryGetFloat(AudioParameterKey paramId, out float result) =>
            this.floatParameters.TryGetValue(paramId.id, out result);

        public bool GetBool(AudioParameterKey paramId) => 
                this.boolParameters[paramId.id];
        
        public int GetInt(AudioParameterKey paramId) => 
                this.intParameters[paramId.id];
        
        public float GetFloat(AudioParameterKey paramId) => 
                this.floatParameters[paramId.id];

        public bool HasFloat(AudioParameterKey paramId) => 
                this.floatParameters.ContainsKey(paramId.id);
        
        public bool HasInt(AudioParameterKey paramId) => 
                this.intParameters.ContainsKey(paramId.id);
        
        public bool HasBool(AudioParameterKey paramId) => 
                this.boolParameters.ContainsKey(paramId.id);

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetBool(AudioParameterKey paramId, bool value) => this.boolParameters[paramId.id] = value;

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetInt(AudioParameterKey paramId, int value) => this.intParameters[paramId.id] = value;

#if ODIN_INSPECTOR
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
#endif
        public void SetFloat(AudioParameterKey paramId, float value) => this.floatParameters[paramId.id] = value;
    }
}