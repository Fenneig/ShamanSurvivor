using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class TidalAuraView : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer _surface;
        [SerializeField] private SpriteRenderer _ripple1;
        [SerializeField] private SpriteRenderer _ripple2;

        [Header("Ripple Shape")]
        [SerializeField, Range(0f, 1f)] private float _startRadius = 0.1f;
        [SerializeField, Range(0f, 1f)] private float _endRadius = 1f;
        [SerializeField, Range(0f, 1f)] private float _secondRippleOffset = 0.5f;
        [Header("Ripple Opacity")] 
        [SerializeField, Range(0f, 1f)] private float _maxRippleOpacity = 0.8f;
        [SerializeField, Range(0.01f, 0.5f)] private float _fadeInPortion = 0.12f;
        [SerializeField, Range(0.01f, 0.9f)] private float _fadeOutStart = 0.55f;
        [Header("Rotation (Degrees Per Second)")]
        [SerializeField] private float _surfaceRotationSpeed = 5f;
        [SerializeField] private float _ripple1RotationSpeed = -15f;
        [SerializeField] private float _ripple2RotationSpeed = 12f;
        [Header("Debug Hitbox")]
        [SerializeField] private SpriteRenderer _debugHitbox;
        
        private Color _ripple1Color;
        private Color _ripple2Color;

        private float _previousRadius = -1f;

        private float _surfaceAngle;
        private float _ripple1Angle;
        private float _ripple2Angle;

        private Quaternion _surfaceInitialRotation;
        private Quaternion _ripple1InitialRotation;
        private Quaternion _ripple2InitialRotation;
        
        private void Awake()
        {
            _ripple1Color = _ripple1.color;
            _ripple2Color = _ripple2.color;
            
            _surfaceInitialRotation = _surface.transform.localRotation;
            _ripple1InitialRotation = _ripple1.transform.localRotation;
            _ripple2InitialRotation = _ripple2.transform.localRotation;
        }

        public void Show(Vector2 position, float radius, float cooldownPhase, float zPosition)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            transform.position = new Vector3(position.x, position.y, zPosition);
            radius = Mathf.Max(0.01f, radius);
            
            if (!Mathf.Approximately(radius, _previousRadius))
            {
                _previousRadius = radius;

                SetDiameter(_surface, radius * 2f);
            }

            float phase1 = Mathf.Repeat(cooldownPhase, 1f);
            float phase2 = Mathf.Repeat(cooldownPhase + _secondRippleOffset, 1f);

            UpdateRipple(_ripple1, _ripple1Color, radius, phase1);
            UpdateRipple(_ripple2, _ripple2Color, radius, phase2);
            
            UpdateDebugHitbox(radius);
            RotateElements(Time.deltaTime);
        }

        private void UpdateRipple(SpriteRenderer renderer, Color baseColor, float auraRadius, float phase)
        {
            float radiusFraction = Mathf.Lerp(_startRadius, _endRadius, phase);
            float diameter = auraRadius * 2f * radiusFraction;

            SetDiameter(renderer, diameter);

            float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(phase / _fadeInPortion));
            float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_fadeOutStart, 1f, phase));
            float alpha = fadeIn * fadeOut * _maxRippleOpacity;
            baseColor.a *= alpha;
            renderer.color = baseColor;
        }

        private static void SetDiameter(SpriteRenderer renderer, float diameter)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            Vector2 spriteSize = renderer.sprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            renderer.transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
        
        private void UpdateDebugHitbox(float radius)
        {
            if (_debugHitbox == null || !_debugHitbox.gameObject.activeSelf || _debugHitbox.sprite == null)
                return;

            Vector2 spriteSize = _debugHitbox.sprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            float diameter = radius * 2f;
            _debugHitbox.transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        }
        private void RotateElements(float deltaTime)
        {
            _surfaceAngle = Mathf.Repeat(_surfaceAngle + _surfaceRotationSpeed * deltaTime, 360f);
            _ripple1Angle = Mathf.Repeat(_ripple1Angle + _ripple1RotationSpeed * deltaTime, 360f);
            _ripple2Angle = Mathf.Repeat(_ripple2Angle + _ripple2RotationSpeed * deltaTime, 360f);

            _surface.transform.localRotation = _surfaceInitialRotation * Quaternion.Euler(0f, 0f, _surfaceAngle);
            _ripple1.transform.localRotation = _ripple1InitialRotation * Quaternion.Euler(0f, 0f, _ripple1Angle);
            _ripple2.transform.localRotation = _ripple2InitialRotation * Quaternion.Euler(0f, 0f, _ripple2Angle);
        }
    }
}