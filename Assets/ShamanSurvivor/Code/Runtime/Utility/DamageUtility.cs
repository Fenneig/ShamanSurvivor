using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public static class DamageUtility
    {
        public static bool TryAddDamage(Entity target,
            ref ComponentLookup<Health> healthLookup,
            ref BufferLookup<DamageEvent> damageLookup,
            ref ComponentLookup<Dead> deadLookup,
            in DamageEvent damageEvent)
        {
            if (!healthLookup.HasComponent(target) ||
                !damageLookup.HasBuffer(target) ||
                !deadLookup.HasComponent(target))
                return false;

            if (deadLookup.IsComponentEnabled(target))
                return false;

            DynamicBuffer<DamageEvent> damageBuffer = damageLookup[target];

            float remainingHealth = healthLookup[target].Current;

            for (int i = 0; i < damageBuffer.Length; i++) 
                remainingHealth -= math.max(0f, damageBuffer[i].Amount);

            damageBuffer.Add(damageEvent);

            remainingHealth -= math.max(0f, damageEvent.Amount);

            if (remainingHealth <= 0f) 
                deadLookup.SetComponentEnabled(target, true);

            return true;
        }
    }
}