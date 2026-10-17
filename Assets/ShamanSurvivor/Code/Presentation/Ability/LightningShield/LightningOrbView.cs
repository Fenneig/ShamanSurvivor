using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class LightningOrbView : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer _glow;
        [SerializeField] private SpriteRenderer _core;
        [SerializeField] private SpriteRenderer _electricity;
        [SerializeField] private SpriteRenderer _spark;

        [Header("Electricity Frames")]
        [SerializeField] private Sprite[] _electricityFrames;
        [SerializeField, Min(1f)] private float _electricityFps = 12f;

        [Header("Scale")]
        [SerializeField] private float _baseDiameter = 1f;
        [SerializeField] private float _glowScaleMultiplier = 1.35f;
        [SerializeField] private float _coreScaleMultiplier = 0.95f;
        [SerializeField] private float _electricityScaleMultiplier = 1.10f;
        [SerializeField] private float _sparkScaleMultiplier = 0.65f;

        [Header("Pulse")]
        [SerializeField] private float _pulseSpeed = 4f;
        [SerializeField] private float _glowPulseAmplitude = 0.08f;
        [SerializeField] private float _corePulseAmplitude = 0.04f;
        [SerializeField] private float _electricityPulseAmplitude = 0.05f;

        [Header("Rotation")]
        [SerializeField] private float _glowRotationSpeed = 8f;
        [SerializeField] private float _coreRotationSpeed = -15f;
        [SerializeField] private float _electricityRotationSpeed = 35f;
        [SerializeField] private float _sparkRotationSpeed = -80f;

        [Header("Spark")]
        [SerializeField] private bool _useSpark = true;
        [SerializeField] private float _sparkBlinkChancePerSecond = 1.5f;
        [SerializeField] private float _sparkBlinkDuration = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _sparkAlpha = 0.85f;

        private float _diameter;
        private float _time;
        private float _electricityFrameTimer;
        private int _electricityFrameIndex;

        private float _glowAngle;
        private float _coreAngle;
        private float _electricityAngle;
        private float _sparkAngle;

        private float _sparkTimer;
        private bool _sparkVisible;

        private Color _glowBaseColor;
        private Color _coreBaseColor;
        private Color _electricityBaseColor;
        private Color _sparkBaseColor;

        private Quaternion _glowBaseRotation;
        private Quaternion _coreBaseRotation;
        private Quaternion _electricityBaseRotation;
        private Quaternion _sparkBaseRotation;

        private void Awake()
        {
            if (_glow != null)
            {
                _glowBaseColor = _glow.color;
                _glowBaseRotation = _glow.transform.localRotation;
            }
            if (_core != null)
            {
                _coreBaseColor = _core.color;
                _coreBaseRotation = _core.transform.localRotation;
            }
            if (_electricity != null)
            {
                _electricityBaseColor = _electricity.color;
                _electricityBaseRotation = _electricity.transform.localRotation;
            }
            if (_spark != null)
            {
                _sparkBaseColor = _spark.color;
                _sparkBaseRotation = _spark.transform.localRotation;
                _spark.enabled = false;
            }
        }

        public void Show(Vector2 position, float diameter, float zPosition)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            transform.position = new Vector3(position.x, position.y, zPosition);

            _diameter = Mathf.Max(0.01f, diameter);

            ApplyDiameter(_glow, _diameter * _glowScaleMultiplier);
            ApplyDiameter(_core, _diameter * _coreScaleMultiplier);
            ApplyDiameter(_electricity, _diameter * _electricityScaleMultiplier);
            ApplyDiameter(_spark, _diameter * _sparkScaleMultiplier);
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;

            UpdatePulse();
            UpdateRotation(deltaTime);
            UpdateElectricity(deltaTime);
            UpdateSpark(deltaTime);
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            _sparkTimer = 0f;
            _sparkVisible = false;

            if (_spark != null)
                _spark.enabled = false;
        }

        private void UpdatePulse()
        {
            float pulse01 = (Mathf.Sin(_time * _pulseSpeed) + 1f) * 0.5f;
            float pulseSigned = pulse01 * 2f - 1f;

            ApplyDiameter(_glow, _diameter * _glowScaleMultiplier * (1f + pulseSigned * _glowPulseAmplitude));
            ApplyDiameter(_core, _diameter * _coreScaleMultiplier * (1f + pulseSigned * _corePulseAmplitude));
            ApplyDiameter(_electricity, _diameter * _electricityScaleMultiplier * (1f + pulseSigned * _electricityPulseAmplitude));

            if (_glow != null)
            {
                Color c = _glowBaseColor;
                c.a *= 0.85f + pulse01 * 0.15f;
                _glow.color = c;
            }

            if (_electricity != null)
            {
                Color c = _electricityBaseColor;
                c.a *= 0.80f + pulse01 * 0.20f;
                _electricity.color = c;
            }
        }

        private void UpdateRotation(float deltaTime)
        {
            _glowAngle = Mathf.Repeat(_glowAngle + _glowRotationSpeed * deltaTime, 360f);
            _coreAngle = Mathf.Repeat(_coreAngle + _coreRotationSpeed * deltaTime, 360f);
            _electricityAngle = Mathf.Repeat(_electricityAngle + _electricityRotationSpeed * deltaTime, 360f);
            _sparkAngle = Mathf.Repeat(_sparkAngle + _sparkRotationSpeed * deltaTime, 360f);

            if (_glow != null)
                _glow.transform.localRotation = _glowBaseRotation * Quaternion.Euler(0f, 0f, _glowAngle);

            if (_core != null)
                _core.transform.localRotation = _coreBaseRotation * Quaternion.Euler(0f, 0f, _coreAngle);

            if (_electricity != null)
                _electricity.transform.localRotation = _electricityBaseRotation * Quaternion.Euler(0f, 0f, _electricityAngle);

            if (_spark != null)
                _spark.transform.localRotation = _sparkBaseRotation * Quaternion.Euler(0f, 0f, _sparkAngle);
        }

        private void UpdateElectricity(float deltaTime)
        {
            if (_electricity == null || _electricityFrames == null || _electricityFrames.Length == 0)
                return;

            _electricityFrameTimer += deltaTime;
            float frameDuration = 1f / Mathf.Max(1f, _electricityFps);

            while (_electricityFrameTimer >= frameDuration)
            {
                _electricityFrameTimer -= frameDuration;
                _electricityFrameIndex = (_electricityFrameIndex + 1) % _electricityFrames.Length;
            }

            _electricity.sprite = _electricityFrames[_electricityFrameIndex];
        }

        private void UpdateSpark(float deltaTime)
        {
            if (!_useSpark || _spark == null)
                return;

            if (_sparkVisible)
            {
                _sparkTimer -= deltaTime;

                if (_sparkTimer <= 0f)
                {
                    _sparkVisible = false;
                    _spark.enabled = false;
                }

                return;
            }

            float chance = _sparkBlinkChancePerSecond * deltaTime;

            if (Random.value <= chance)
            {
                _sparkVisible = true;
                _sparkTimer = _sparkBlinkDuration;
                _spark.enabled = true;

                Color c = _sparkBaseColor;
                c.a *= _sparkAlpha;
                _spark.color = c;
            }
        }

        private void ApplyDiameter(SpriteRenderer renderer, float diameter)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            Vector2 spriteSize = renderer.sprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            renderer.transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        }
    }
}