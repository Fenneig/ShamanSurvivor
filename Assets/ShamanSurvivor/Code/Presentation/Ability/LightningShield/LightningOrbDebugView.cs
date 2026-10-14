using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class LightningOrbDebugView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _hitboxFill;
        [SerializeField] private LineRenderer _hitboxOutline;

        [SerializeField, Range(16, 128)]
        private int _segments = 48;
        private float _previousRadius = -1f;

        private void Awake()
        {
            if (_hitboxOutline == null)
                return;

            _hitboxOutline.useWorldSpace = false;
            _hitboxOutline.loop = true;
        }

        public void Sync(Vector2 position, float radius, float zPosition)
        {
            transform.position = new Vector3(position.x, position.y, zPosition);

            radius = Mathf.Max(0.01f, radius);

            if (Mathf.Abs(radius - _previousRadius) < 0.0001f)
                return;

            _previousRadius = radius;

            UpdateFill(radius);
            DrawCircle(_hitboxOutline, radius, _segments);
        }

        private void UpdateFill(float radius)
        {
            if (_hitboxFill == null || _hitboxFill.sprite == null)
                return;

            Vector2 spriteSize = _hitboxFill.sprite.bounds.size;
            float diameter = radius * 2f;
            _hitboxFill.transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        }

        public static void DrawCircle(LineRenderer line, float radius, int segments)
        {
            if (line == null)
                return;

            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;

                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }
    }
}