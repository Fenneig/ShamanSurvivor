using ShamanSurvivor.Runtime;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class EarthquakeZoneView : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer _ground;
        [SerializeField] private SpriteRenderer _cracks;
        [Header("Rings")]
        [SerializeField] private LineRenderer _boundaryRing;
        [SerializeField] private LineRenderer _pulseRing;

        [SerializeField, Range(12, 128)] private int _segments = 48;
        [SerializeField] private float _boundaryWidth = 0.045f;
        [SerializeField] private float _pulseWidth = 0.10f;

        [Header("Colors")]
        [SerializeField] private Color _groundColor = new(0.45f, 0.30f, 0.17f, 0.30f);
        [SerializeField] private Color _crackColor = new(0.90f, 0.55f, 0.25f, 0.75f);
        [SerializeField] private Color _boundaryColor = new(0.85f, 0.60f, 0.35f, 0.50f);
        [SerializeField] private Color _pulseColor = new(1f, 0.75f, 0.40f, 0.85f);
        [Header("Lifetime")]
        [SerializeField] private float _fadeOutDuration = 0.3f;

        private Vector2[] _unitCircle;
        private Vector3[] _positions;

        private float _lastRadius = -1f;

        private void Awake()
        {
            int count = Mathf.Max(12, _segments);

            _unitCircle = new Vector2[count];
            _positions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;

                _unitCircle[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            ConfigureRing(_boundaryRing, _boundaryWidth);
            ConfigureRing(_pulseRing, _pulseWidth);
        }

        private void ConfigureRing(LineRenderer line, float width)
        {
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = _unitCircle.Length;
            line.widthMultiplier = width;
        }

        public void Sync(in EarthquakeZone zone, float zPosition)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            transform.position = new Vector3(zone.Position.x, zone.Position.y, zPosition);

            float radius = Mathf.Max(0.01f, zone.Radius);

            if (!Mathf.Approximately(_lastRadius, radius))
            {
                _lastRadius = radius;

                SetSpriteDiameter(_ground, radius * 2f);
                SetSpriteDiameter(_cracks, radius * 2f);

                BuildCircle(_boundaryRing, radius);
            }

            float interval = Mathf.Max(0.01f, zone.TickInterval);
            float phase = 1f - Mathf.Clamp01(zone.TickTimer / interval);
            float pulseRadius = radius * Mathf.Lerp(0.15f, 1f, phase);

            BuildCircle(_pulseRing, pulseRadius);

            float pulseAlpha = Mathf.Pow(1f - phase, 1.5f);
            float impactBrightness = Mathf.Pow(1f - phase, 3f);
            float fade = Mathf.Clamp01(zone.RemainingDuration / Mathf.Max(0.01f, _fadeOutDuration));

            SetSpriteColor(_ground, _groundColor, fade);
            SetSpriteColor(_cracks, _crackColor, fade * (0.55f + 0.45f * impactBrightness));
            SetLineColor(_boundaryRing, _boundaryColor, fade);
            SetLineColor(_pulseRing, _pulseColor, fade * pulseAlpha);
        }

        private void BuildCircle(LineRenderer line, float radius)
        {
            for (int i = 0; i < _unitCircle.Length; i++)
            {
                Vector2 point = _unitCircle[i] * radius;
                _positions[i] = new Vector3(point.x, point.y, 0f);
            }

            line.SetPositions(_positions);
        }

        private static void SetSpriteDiameter(SpriteRenderer renderer, float diameter)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            Vector2 spriteSize = renderer.sprite.bounds.size;

            renderer.transform.localScale = new Vector3(
                diameter / spriteSize.x,
                diameter / spriteSize.y,
                1f);
        }

        private static void SetSpriteColor(SpriteRenderer renderer, Color color, float alpha)
        {
            if (renderer == null)
                return;

            color.a *= alpha;
            renderer.color = color;
        }

        private static void SetLineColor(LineRenderer renderer, Color color, float alpha)
        {
            color.a *= alpha;

            renderer.startColor = color;
            renderer.endColor = color;
        }

        public void Hide()
        {
            _lastRadius = -1f;
            gameObject.SetActive(false);
        }
    }
}