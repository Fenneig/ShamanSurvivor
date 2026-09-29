using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public static class PassiveProgressUtility
    {
        public static float GetBonus(DynamicBuffer<PassiveProgress> progress, GlobalPassiveId passive)
        {
            for (int i = 0; i < progress.Length; i++)
            {
                if (progress[i].Passive == passive)
                    return progress[i].TotalBonus;
            }

            return 0f;
        }
    }
}