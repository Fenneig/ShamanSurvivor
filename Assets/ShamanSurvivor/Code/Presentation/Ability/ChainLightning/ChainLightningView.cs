using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class ChainLightningView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private ChainLightningEffectPool _pool;
        private Vector3 _from;
        private Vector3 _to;
        private float _delayRemaining;
        private float _lifetime;
        private float _remainingLifetime;
        private float _jitter;
        private float _flickerInterval;
        private float _flickerTimer;
        private int _pointCount;
        private bool _started;

        private Color _baseStartColor;
        private Color _baseEndColor;

        public void Initialize(ChainLightningEffectPool pool)
        {
            _pool = pool;

            _baseStartColor = _lineRenderer.startColor;

            _baseEndColor = _lineRenderer.endColor;
        }

        public void Play(
            Vector3 from,
            Vector3 to,
            float delay,
            float lifetime,
            float width,
            float jitter,
            int pointCount,
            float flickerInterval)
        {
            _from = from;
            _to = to;

            _delayRemaining = Mathf.Max(0f, delay);
            _lifetime = Mathf.Max(0.01f, lifetime);
            _remainingLifetime = _lifetime;
            _jitter = Mathf.Max(0f, jitter);
            _pointCount = Mathf.Max(2, pointCount);

            _flickerInterval = Mathf.Max(0.01f, flickerInterval);
            _flickerTimer = 0f;
            _started = false;
            _lineRenderer.enabled = false;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            gameObject.SetActive(true);

            if (_delayRemaining <= 0f) 
                StartEffect();
        }
        
        private void Update()
        {
            if (!_started)
            {
                _delayRemaining -= Time.deltaTime;
                
                if (_delayRemaining <= 0) 
                    StartEffect();

                return;
            }

            _remainingLifetime -= Time.deltaTime;

            if (_remainingLifetime <= 0f)
            {
                _pool.Release(this);
                return;
            }

            _flickerTimer -= Time.deltaTime;

            if (_flickerTimer <= 0f)
            {
                BuildLightning();

                _flickerTimer = _flickerInterval;
            }

            UpdateFade();
        }

        private void StartEffect()
        {
            _started = true;
            _lineRenderer.enabled = true;
            BuildLightning();
            _flickerTimer = _flickerInterval;
        }

        private void BuildLightning()
        {
            _lineRenderer.positionCount = _pointCount;

            Vector3 delta = _to - _from;
            Vector3 direction = delta.sqrMagnitude > 0.0001f
                    ? delta.normalized
                    : Vector3.right;

            Vector3 side = new Vector3(-direction.y, direction.x, 0f);

            for (int i = 0; i < _pointCount; i++)
            {
                float t = i / (float)(_pointCount - 1);

                Vector3 point = Vector3.Lerp(_from, _to, t);

                if (i != 0 && i != _pointCount - 1)
                {
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float offset = Random.Range(-_jitter, _jitter) * envelope;
                    point += side * offset;
                }

                _lineRenderer.SetPosition(i, point);
            }
        }

        private void UpdateFade()
        {
            float alpha = Mathf.Clamp01(_remainingLifetime / _lifetime);
            Color startColor = _baseStartColor;
            startColor.a *= alpha;
            Color endColor = _baseEndColor;
            endColor.a *= alpha;
            _lineRenderer.startColor = startColor;
            _lineRenderer.endColor = endColor;
        }

        public void ResetView()
        {
            _started = false;
            _lineRenderer.enabled = false;
            _lineRenderer.startColor = _baseStartColor;
            _lineRenderer.endColor = _baseEndColor;
            gameObject.SetActive(false);
        }
    }
}