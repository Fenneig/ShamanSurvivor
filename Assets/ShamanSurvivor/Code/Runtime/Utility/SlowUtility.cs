using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public static class SlowUtility
    {
        public static void ApplyOrRefresh(DynamicBuffer<Slow> effects, Entity source, float amount, float duration)
        {
            amount = math.clamp(amount, 0f, 1f);

            duration = math.max(0f, duration);
            
            for (int i = 0; i < effects.Length; i++)
            {
                Slow effect = effects[i];
                
                if (effect.Source != source)
                    continue;
                
                effect.Amount = amount;
                effect.RemainingDuration = math.max(effect.RemainingDuration, duration);

                effects[i] = effect;
                return;
            }

            effects.Add(new Slow
            {
                Source = source,
                RemainingDuration = duration,
                Amount = amount
            });
        }

        public static float GetStrongest(DynamicBuffer<Slow> effects)
        {
            float strongest = 0f;

            for (int i = 0; i < effects.Length; i++)
            {
                Slow effect = effects[i];

                if (effect.RemainingDuration <= 0f)
                    continue;

                strongest = math.max(strongest, effect.Amount);
            }

            return strongest;
            
        }
    }
}