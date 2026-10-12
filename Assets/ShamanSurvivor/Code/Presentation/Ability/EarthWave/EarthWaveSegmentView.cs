using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class EarthWaveSegmentView : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer _ridge;
        [SerializeField] private SpriteRenderer _cracks;
        [SerializeField] private SpriteRenderer _dust;
        [Header("Ridge Animations")]
        [SerializeField] private Sprite[] _mainFrames;
        [SerializeField] private Sprite[] _mainFramesB;
        [SerializeField] private Sprite[] _sideFrames;
        [SerializeField] private Sprite[] _sideFramesB;
        [Header("Ground")]
        [SerializeField] private Sprite[] _crackVariants;
        [SerializeField] private Sprite[] _dustFrames;
        [Header("Scale Progression")]
        [SerializeField, Min(0f)] private float _mainRidgeGrowth = 0.6f;
        [SerializeField, Min(0f)] private float _sideRidgeGrowth = 0.4f;
        [SerializeField, Min(0f)] private float _cracksGrowth = 0.8f;
        [SerializeField, Min(0f)] private float _dustGrowth = 0.3f;

        private Sprite[] _currentRidgeFrames;

        private float _elapsed;
        private float _lifetime;

        private Color _ridgeColor;
        private Color _crackColor;
        private Color _dustColor;
        
        private Vector3 _ridgeBaseScale;
        private Vector3 _cracksBaseScale;
        private Vector3 _dustBaseScale;

        private void Awake()
        {
            _ridgeColor = _ridge.color;
            _crackColor = _cracks.color;
            _dustColor = _dust.color;
            
            _ridgeBaseScale = _ridge.transform.localScale;
            _cracksBaseScale = _cracks.transform.localScale;
            _dustBaseScale = _dust.transform.localScale;
        }

        public void Play(Vector2 position, Vector2 direction, bool isMain, float scale, float sizeMultiplier, float lifetime, float zPosition)    
        {
            transform.position = new Vector3(position.x, position.y, zPosition);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.localScale = Vector3.one * scale;
            ApplySizeProgression(isMain, sizeMultiplier);
            
            _lifetime = Mathf.Max(0.01f, lifetime);
            _elapsed = 0f;

            _currentRidgeFrames = SelectFrames(isMain);

            if (_crackVariants != null && _crackVariants.Length > 0)
            {
                int index = Random.Range(0, _crackVariants.Length);
                _cracks.sprite = _crackVariants[index];
            }
            else
            {
                _cracks.sprite = null;
            }

            _ridge.enabled = _currentRidgeFrames != null && _currentRidgeFrames.Length > 0;
            _cracks.enabled = _cracks.sprite != null;
            _dust.enabled = _dustFrames != null && _dustFrames.Length > 0;
            gameObject.SetActive(true);
            Tick(0f);
        }

        private void ApplySizeProgression(bool isMain, float sizeMultiplier)
        {
            float sizeBonus = Mathf.Max(0f, sizeMultiplier - 1f);

            float ridgeGrowth = isMain
                    ? _mainRidgeGrowth
                    : _sideRidgeGrowth;

            float ridgeScale = _ridgeBaseScale.x + sizeBonus * ridgeGrowth;
            float cracksScale = _cracksBaseScale.x + sizeBonus * _cracksGrowth;
            float dustScale = _dustBaseScale.x + sizeBonus * _dustGrowth;

            _ridge.transform.localScale = _ridgeBaseScale * ridgeScale;
            _cracks.transform.localScale = _cracksBaseScale * cracksScale;
            _dust.transform.localScale = _dustBaseScale * dustScale;
        }

        public bool Tick(float deltaTime)
        {
            _elapsed += deltaTime;

            if (_elapsed >= _lifetime)
                return false;

            float t = Mathf.Clamp01(_elapsed / _lifetime);

            if (_ridge.enabled)
            {
                int index = Mathf.Min(Mathf.FloorToInt(t * _currentRidgeFrames.Length), _currentRidgeFrames.Length - 1);

                _ridge.sprite = _currentRidgeFrames[index];
            }

            if (_dust.enabled)
            {
                float dustProgress = Mathf.Clamp01(t / 0.7f);

                int index = Mathf.Min(Mathf.FloorToInt(dustProgress * _dustFrames.Length), _dustFrames.Length - 1);

                _dust.sprite = _dustFrames[index];
            }

            float appear = Mathf.Clamp01(t / 0.15f);
            float disappear = Mathf.Clamp01((1f - t) / 0.30f);
            float alpha = appear * disappear;

            SetAlpha(_ridge, _ridgeColor, alpha);
            SetAlpha(_cracks, _crackColor, disappear);

            SetAlpha(_dust, _dustColor, Mathf.Clamp01(1f - t / 0.7f));

            return true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private Sprite[] SelectFrames(bool isMain)
        {
            Sprite[] primary = isMain ? _mainFrames : _sideFrames;
            Sprite[] variant = isMain ? _mainFramesB : _sideFramesB;

            if (variant != null && variant.Length > 0 && Random.value < 0.5f)
                return variant;

            return primary;
        }

        private static void SetAlpha(SpriteRenderer renderer, Color originalColor, float alpha)
        {
            originalColor.a *= alpha;
            renderer.color = originalColor;
        }
    }
}