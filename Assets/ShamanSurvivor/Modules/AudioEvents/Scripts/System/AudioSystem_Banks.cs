using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Modules.AudioEvents
{
    public partial class AudioSystem
    {
#if ODIN_INSPECTOR
        [HideInPlayMode]
#endif
        [Header("Setup")]
        [SerializeField]
        private AudioBank[] initialBanks;

        private readonly HashSet<AudioBank> _registeredBanks = new();

        public bool RegisterBank(AudioBank bank)
        {
            if (bank == null)
                return false;

            if (!_registeredBanks.Add(bank))
                return false;

            string bankIdentifier = bank.Identifier;

            foreach (AudioBank.Entry<AudioEvent> eventParameter in bank.Events)
            {
                int id = AudioIdStore.NameToId($"{bankIdentifier}.{eventParameter.name}");
                this.audioEventPool.Register(id, eventParameter.value);
            }

            foreach (AudioBank.Entry<bool> boolParameter in bank.BoolParameters)
            {
                int id = AudioIdStore.NameToId($"{bankIdentifier}.{boolParameter.name}");
                this.boolParameters[id] = boolParameter.value;
            }

            foreach (AudioBank.Entry<int> intParameter in bank.IntParameters)
            {
                int id = AudioIdStore.NameToId($"{bankIdentifier}.{intParameter.name}");
                this.intParameters[id] = intParameter.value;
            }

            foreach (AudioBank.Entry<float> floatParameter in bank.FloatParameters)
            {
                int id = AudioIdStore.NameToId($"{bankIdentifier}.{floatParameter.name}");
                this.floatParameters[id] = floatParameter.value;
            }

            return true;
        }

        public bool UnregisterBank(AudioBank bank)
        {
            if (bank == null || !_registeredBanks.Remove(bank))
                return false;

            string bankName = bank.Identifier;

            foreach (AudioBank.Entry<AudioEvent> eventParameter in bank.Events)
            {
                int id = AudioIdStore.NameToId($"{bankName}.{eventParameter.name}");
                this.audioEventPool.Unregister(id);
            }

            foreach (AudioBank.Entry<bool> boolParameter in bank.BoolParameters)
            {
                int id = AudioIdStore.NameToId($"{bankName}.{boolParameter.name}");
                this.boolParameters.Remove(id);
            }

            foreach (AudioBank.Entry<int> intParameter in bank.IntParameters)
            {
                int id = AudioIdStore.NameToId($"{bankName}.{intParameter.name}");
                this.intParameters.Remove(id);
            }

            foreach (AudioBank.Entry<float> floatParameter in bank.FloatParameters)
            {
                int id = AudioIdStore.NameToId($"{bankName}.{floatParameter.name}");
                this.floatParameters.Remove(id);
            }

            return true;
        }

        private void RegisterInitialBanks()
        {
            foreach (AudioBank bank in this.initialBanks)
                if (bank != null)
                    this.RegisterBank(bank);
        }
    }
}