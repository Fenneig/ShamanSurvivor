using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public static class SearchAbilityUtility
    {
        public static int FindAbilityIndex(DynamicBuffer<AbilityState> abilities, AbilityId ability)
        {
            for (int i = 0; i < abilities.Length; i++)
                if (abilities[i].Ability == ability)
                    return i;

            return -1;
        }
        
        public static AbilityDefinition FindAbilityDefinition(DynamicBuffer<AbilityDefinition> abilities, AbilityId ability)
        {
            for (int i = 0; i < abilities.Length; i++)
                if (abilities[i].Ability == ability)
                    return abilities[i];

            return default;
        }

        public static bool TryGetDefinition(DynamicBuffer<AbilityDefinition> definitions, AbilityId ability, out AbilityDefinition abilityDefinition)
        {
            foreach (var definition in definitions)
            {
                if (definition.Ability == ability)
                {
                    abilityDefinition = definition;
                    return true;
                }
            }
            
            abilityDefinition = default;
            return false;
        }
    }
}