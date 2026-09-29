using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "XpOrbConfig", menuName = "Shaman Survivor/Xp Orb Config")]
    public class XpOrbConfig : GameConfig
    {
        [Header("Pickup")]
        [Min(0f)]
        [SerializeField] public float MagnetRadius;
        [Min(0f)] 
        [SerializeField] public float CollectRadius;
        [Min(0f)]
        [SerializeField] public float MoveSpeed;
    }
}