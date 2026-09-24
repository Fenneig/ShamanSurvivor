#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using static Modules.AudioEvents.InternalUtils;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public partial class AudioBank
    {
        [Header("Codegen")]
        [SerializeField]
        private string _namespace = "Modules.AudioEvents";

        [SerializeField]
        private string _directoryPath = "Assets/Audio";

#if ODIN_INSPECTOR
        [Button]
        [PropertySpace]
        [GUIColor(1f, 0.83f, 0f)]
#endif
        [ContextMenu("Compile")]
        private void Compile()
        {
            string bankName = this.Identifier;
            string className = $"{ToTitleCase(bankName)}AudioKeys";
            string selectedPath = $"{_directoryPath}/{className}.cs";

            using StreamWriter writer = new StreamWriter(selectedPath);

            writer.WriteLine("/**");
            writer.WriteLine("* Code generation. Don't modify! ");
            writer.WriteLine(" */");

            writer.WriteLine();

            writer.WriteLine($"namespace {_namespace}");
            writer.WriteLine("{");
            writer.WriteLine($"    public static class {className}");
            writer.WriteLine("    {");

            writer.WriteLine("        ///Events");

            //Generate event ids:
            foreach (Entry<AudioEvent> entry in _events)
                writer.WriteLine(
                    $"        public static readonly AudioEventKey {ToTitleCase(entry.name)}Event = new AudioEventKey(\"{bankName}.{entry.name}\");");

            writer.WriteLine();

            //Generate event ids:
            writer.WriteLine("        ///Parameters");
            foreach (string parameter in this.GetAllParameterNames())
                writer.WriteLine(
                    $"        public static readonly AudioParameterKey {ToTitleCase(parameter)}Parameter = new AudioParameterKey(\"{bankName}.{parameter}\");");

            writer.WriteLine();

            //Generate callbacks:
            writer.WriteLine("        ///Callbacks");
            foreach (string callback in _callbacks)
                writer.WriteLine(
                    $"        public static readonly AudioCallbackKey {ToTitleCase(callback)}Callback = new AudioCallbackKey(\"{bankName}.{callback}\");");

            writer.WriteLine("    }");
            writer.WriteLine("}");

            AssetDatabase.Refresh();
        }
    }
}
#endif