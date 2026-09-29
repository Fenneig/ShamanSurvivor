using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public static class UpgradeProgressUtility
    {
        public static float GetBonus(DynamicBuffer<UpgradeProgress> progress, AbilityId ability, UpgradeKey key)
        {
            for (int i = 0; i < progress.Length; i++)
            {
                UpgradeProgress current = progress[i];

                if (current.Ability == ability && current.Key == key)
                    return current.TotalBonus;
            }

            return 0f;
        }
    }
}