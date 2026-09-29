using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public static class SpatialQuery
    {
        public static bool TryFindNearest(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid, float2 origin, float range,
            out EnemySpatialEntry nearest)
        {
            nearest = default;

            if (range < 0f || !grid.IsCreated)
                return false;

            float nearestDistanceSq = range * range;

            bool found = false;

            GetCellBounds(origin, range, out int2 minCell, out int2 maxCell);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);

                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry entry, out var iterator))
                        continue;

                    do
                    {
                        float distanceSq = math.distancesq(origin, entry.Position);

                        if (distanceSq > nearestDistanceSq)
                            continue;

                        nearestDistanceSq = distanceSq;

                        nearest = entry;

                        found = true;
                    } while (grid.TryGetNextValue(out entry, ref iterator));
                }
            }

            return found;
        }

        public static bool TryFindNearest(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid, float2 origin, float range, Entity excludedEntity,
            out EnemySpatialEntry nearest)
        {
            nearest = default;

            if (range < 0f || !grid.IsCreated)
                return false;

            float nearestDistanceSq = range * range;

            bool found = false;

            GetCellBounds(origin, range, out int2 minCell, out int2 maxCell);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);

                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry entry, out var iterator))
                        continue;

                    do
                    {
                        if (entry.Entity == excludedEntity)
                            continue;

                        float distanceSq = math.distancesq(origin, entry.Position);

                        if (distanceSq > nearestDistanceSq)
                            continue;

                        nearestDistanceSq = distanceSq;

                        nearest = entry;

                        found = true;
                    } while (grid.TryGetNextValue(out entry, ref iterator));
                }
            }

            return found;
        }

        public static void CollectInRadius(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid,
            float2 origin,
            float radius,
            ref NativeList<EnemySpatialEntry> results)
        {
            results.Clear();

            if (radius < 0f || !grid.IsCreated)
                return;

            float radiusSq =
                radius * radius;

            GetCellBounds(
                origin,
                radius,
                out int2 minCell,
                out int2 maxCell);

            for (int y = minCell.y;
                 y <= maxCell.y;
                 y++)
            {
                for (int x = minCell.x;
                     x <= maxCell.x;
                     x++)
                {
                    int2 cell =
                        new int2(x, y);

                    if (!grid.TryGetFirstValue(
                            cell,
                            out EnemySpatialEntry entry,
                            out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        float distanceSq =
                            math.distancesq(
                                origin,
                                entry.Position);

                        if (distanceSq > radiusSq)
                            continue;

                        results.Add(entry);
                    } while (
                        grid.TryGetNextValue(
                            out entry,
                            ref iterator));
                }
            }
        }

        public static float2 CalculateSeparation(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid,
            Entity self, float2 position, float selfRadius, float searchRadius, float personalSpace)
        {
            if (!grid.IsCreated || searchRadius <= 0f)
                return float2.zero;

            float2 correction = float2.zero;
            
            GetCellBounds(position, searchRadius, out int2 minCell, out int2 maxCell);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);

                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry neighbour, out var iterator))
                        continue;

                    do
                    {
                        if (neighbour.Entity == self)
                            continue;

                        float2 delta = position - neighbour.Position;
                        float desiredDistance = selfRadius + neighbour.Radius + personalSpace;
                        float desiredDistanceSq = desiredDistance * desiredDistance;
                        float distanceSq = math.lengthsq(delta);

                        if (distanceSq >= desiredDistanceSq)
                            continue;
                        
                        if (distanceSq <= 0.000001f)
                        {
                            correction += GetOverlapDirection(self, neighbour.Entity) * (desiredDistance * .5f);
                            
                            continue;
                        }

                        float distance = math.sqrt(distanceSq);

                        float penetration = desiredDistance - distance;
                        float2 direction = delta / distance;

                        correction += direction * penetration * .5f;
                    } while (grid.TryGetNextValue(out neighbour, ref iterator));
                }
            }

            return correction;
        }

        public static bool TryFindFirstSegmentHit(
            NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid, float2 start, float2 end, float radius,
            out Entity hitEntity)
        {
            hitEntity = Entity.Null;

            if (!grid.IsCreated)
                return false;

            float closestHitT = float.MaxValue;
            float2 radiusVector = new float2(radius);
            float2 minPosition = math.min(start, end) - radiusVector;
            float2 maxPosition = math.max(start, end) + radiusVector;
            int2 minCell = EnemySpatialGrid.PositionToCell(minPosition);
            int2 maxCell = EnemySpatialGrid.PositionToCell(maxPosition);
            minCell -= new int2(1);
            maxCell += new int2(1);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);

                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry entry, out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        float combinedRadius = radius + entry.Radius;

                        if (!TrySegmentCircleHit(start, end, entry.Position, combinedRadius, out float hitT))
                            continue;

                        if (hitT >= closestHitT)
                            continue;

                        closestHitT = hitT;
                        hitEntity = entry.Entity;
                    } while (grid.TryGetNextValue(out entry, ref iterator));
                }
            }
            return hitEntity != Entity.Null;
        }

        private static bool TrySegmentCircleHit(float2 start, float2 end, float2 center, float radius, out float hitT)
        {
            float2 fromCenter = start - center;
            float radiusSq = radius * radius;

            if (math.lengthsq(fromCenter) <= radiusSq)
            {
                hitT = 0f;
                return true;
            }

            float2 direction = end - start;
            float segmentLengthSq = math.lengthsq(direction);

            if (segmentLengthSq <= 0.000001f)
            {
                hitT = 0f;
                return false;
            }

            float b = math.dot(fromCenter, direction);
            float c = math.dot(fromCenter, fromCenter) - radiusSq;
            float discriminant = b * b - segmentLengthSq * c;

            if (discriminant < 0f)
            {
                hitT = 0f;
                return false;
            }

            float t = (-b - math.sqrt(discriminant)) / segmentLengthSq;

            if (t < 0f || t > 1f)
            {
                hitT = 0f;
                return false;
            }

            hitT = t;
            return true;
        }

        public static bool TryFindFirstDamageableHit(NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly grid,
            float2 start, float2 end, float projectileRadius, ComponentLookup<Health> healthLookup,
            BufferLookup<DamageEvent> damageLookup, out Entity hitEntity)
        {
            hitEntity = Entity.Null;

            if (!grid.IsCreated)
                return false;

            float closestHitT = float.MaxValue;

            float2 radiusVector = new float2(projectileRadius);
            
            float2 minPosition = math.min(start, end) - radiusVector;
            float2 maxPosition = math.max(start, end) + radiusVector;
            
            int2 minCell = EnemySpatialGrid.PositionToCell(minPosition);
            int2 maxCell = EnemySpatialGrid.PositionToCell(maxPosition);
            
            minCell -= new int2(1);
            maxCell += new int2(1);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int2 cell = new int2(x, y);
                    
                    if (!grid.TryGetFirstValue(cell, out EnemySpatialEntry entry, out var iterator))
                        continue;

                    do
                    {
                        if (!CanReceiveDamage(entry.Entity, healthLookup, damageLookup))
                            continue;

                        float combinedRadius = projectileRadius + entry.Radius;

                        if (!TrySegmentCircleHit(start, end, entry.Position, combinedRadius, out float hitT))
                            continue;

                        if (hitT >= closestHitT)
                            continue;

                        closestHitT = hitT;
                        hitEntity = entry.Entity;
                    }
                    while (grid.TryGetNextValue(out entry, ref iterator));
                }
            }

            return hitEntity != Entity.Null;
        }

        private static bool CanReceiveDamage(Entity entity, ComponentLookup<Health> healthLookup, BufferLookup<DamageEvent> damageLookup)
        {
            if (!healthLookup.HasComponent(entity))
                return false;

            if (!damageLookup.HasBuffer(entity))
                return false;
            
            float effectiveHealth = healthLookup[entity].Current;

            if (effectiveHealth <= 0)
                return false;
            
            DynamicBuffer<DamageEvent> pendingDamage= damageLookup[entity];

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

        private static float2 GetOverlapDirection(Entity first, Entity second)
        {
            int minIndex = math.min(first.Index, second.Index);
            int maxIndex = math.max(first.Index, second.Index);
            uint hash = math.hash(new uint2((uint)minIndex, (uint)maxIndex));
            float normalized = (hash & 0x00FFFFFFu) / 16777215f;
            float angle = normalized * math.PI * 2f;
            float2 direction = new float2(math.cos(angle), math.sin(angle));
            
            return first.Index < second.Index
                ? direction
                : -direction;
        }

        private static void GetCellBounds(float2 origin, float radius, out int2 minCell, out int2 maxCell)
        {
            float2 radiusVector = new float2(radius);
            minCell = EnemySpatialGrid.PositionToCell(origin - radiusVector);
            maxCell = EnemySpatialGrid.PositionToCell(origin + radiusVector);
        }
    }
}