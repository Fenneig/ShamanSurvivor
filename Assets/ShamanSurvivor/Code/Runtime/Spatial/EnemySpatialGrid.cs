using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct EnemySpatialEntry
    {
        public Entity Entity;
        public float2 Position;
        public float Radius;
    }
    
    public class EnemySpatialGrid
    {
        public const float CELL_SIZE = 2f;
        
        public const float INVERSE_CELL_SIZE = 1f / CELL_SIZE;

        public static int2 PositionToCell(float2 position) => (int2)math.floor(position * INVERSE_CELL_SIZE);
    }
}