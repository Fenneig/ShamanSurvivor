using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public static class CombatTargetQuery
    {
        public static bool TryFindNearestDamageable(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid,
            float2 origin, float range, ComponentLookup<Dead> deadLookup, in FixedList128Bytes<Entity> excluded, out EnemySpatialEntry result)
        //ComponentLookup<Health> healthLookup, BufferLookup<DamageEvent> damageLookup
        {
            result = default;

            if (!grid.IsCreated || range <= 0f)
                return false;

            float rangeSq = range * range;
            float closestDistanceSq = float.MaxValue;

            int2 minCell = EnemySpatialGrid.PositionToCell(origin - range);
            int2 maxCell = EnemySpatialGrid.PositionToCell(origin + range);

            bool found = false;

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);

                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry entry, out var iterator))
                        continue;

                    do
                    {
                        if (deadLookup.HasComponent(entry.Entity) && deadLookup.IsComponentEnabled(entry.Entity))
                            continue;
                        
                        if (Contains(excluded, entry.Entity))
                            continue;

                        /*
                        if (!CanReceiveDamage(entry.Entity, healthLookup, damageLookup))
                            continue;
                            */

                        float distanceSq = math.distancesq(origin, entry.Position);

                        if (distanceSq > rangeSq || distanceSq >= closestDistanceSq)
                            continue;

                        closestDistanceSq = distanceSq;
                        result = entry;
                        found = true;
                    }
                    while (grid.TryGetNextValue(out entry, ref iterator));
                }
            }

            return found;
        }

        public static bool CanReceiveDamage(Entity entity, ComponentLookup<Health> healthLookup, BufferLookup<DamageEvent> damageLookup)
        {
            if (!healthLookup.HasComponent(entity))
                return false;

            if (!damageLookup.HasBuffer(entity))
                return false;

            float effectiveHealth = healthLookup[entity].Current;

            if (effectiveHealth <= 0)
                return false;

            DynamicBuffer<DamageEvent> pendingDamage = damageLookup[entity];

            foreach (var damageEvent in pendingDamage)
            {
                if (damageEvent.Amount < 0f)
                    continue;

                effectiveHealth -= damageEvent.Amount;

                if (effectiveHealth <= 0f)
                    return false;
            }

            return true;
        }

        private static bool Contains(in FixedList128Bytes<Entity> list, Entity entity)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == entity)
                    return true;
            }

            return false;
        }
    }
}