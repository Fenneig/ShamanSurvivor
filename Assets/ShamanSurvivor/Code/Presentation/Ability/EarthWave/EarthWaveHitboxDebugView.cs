using Unity.Mathematics;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class EarthWaveHitboxDebugView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _hitbox;

        public bool EnabledInPrefab =>
            _hitbox != null &&
            _hitbox.gameObject.activeSelf;

        public void Sync(float2 position, float2 direction, float halfWidth, float halfDepth, float zPosition)
        {
            transform.position = new Vector3(position.x, position.y, zPosition);

            float angle = math.atan2(direction.y, direction.x);
            transform.rotation = Quaternion.Euler(0f, 0f, math.degrees(angle));

            if (_hitbox.sprite == null)
                return;

            Vector2 spriteSize = _hitbox.sprite.bounds.size;

            _hitbox.transform.localScale = new Vector3(halfDepth * 2f / spriteSize.x, halfWidth * 2f / spriteSize.y, 1f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}