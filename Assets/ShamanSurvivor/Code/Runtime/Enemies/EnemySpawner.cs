using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    public partial struct EnemySpawner : IComponentData
    {
        public Entity Prefab;

        public float SpawnRate;
        
        public float MinSpawnRadius;
        public float MaxSpawnRadius;

        public int MaxAlive;

        public float SpawnAccumulator;

        public uint RandomState;
    }
}