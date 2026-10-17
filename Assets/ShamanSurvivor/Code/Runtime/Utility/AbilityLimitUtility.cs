using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public static class AbilityLimitUtility
    {
        public const int MAX_ACTIVE_ABILITIES = 5;

        public static int CountActiveAbilities(DynamicBuffer<AbilityState> abilities) => 
            abilities.Length;

        public static bool IsUnlocked(DynamicBuffer<AbilityState> abilities, AbilityId abilityId)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Ability == abilityId)
                    return true;
            }

            return false;
        }

        public static bool HasFreeSlot(DynamicBuffer<AbilityState> abilities) => CountActiveAbilities(abilities) < MAX_ACTIVE_ABILITIES;

        public static bool CanOfferAbility(DynamicBuffer<AbilityState> abilities, AbilityId abilityId)
        {
            if (IsUnlocked(abilities, abilityId))
                return true;

            return HasFreeSlot(abilities);
        }
    }
}