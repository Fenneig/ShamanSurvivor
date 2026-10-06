using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class ChainLightningEffectPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ChainLightningEffectPool _pool;
        [Header("Timing")]
        [Min(0.01f)]
        [SerializeField] private float _lifetime = 0.12f;
        [Min(0f)]
        [SerializeField] private float _jumpDelay = 0.035f;
        [Header("Shape")]
        [Min(0.01f)]
        [SerializeField] private float _width = 0.15f;
        [Min(0f)]
        [SerializeField] private float _jitter = 0.25f;
        [Min(2)]
        [SerializeField] private int _pointCount = 7;
        [Min(0.01f)]
        [SerializeField] private float _flickerInterval = 0.025f;
        [Header("World")]
        [SerializeField] private float _zPosition = -0.25f;

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _eventQuery;
        private bool _initialized;


        private void Update()
        {
            if (!TryInitialize())
                return;

            if (_eventQuery.IsEmptyIgnoreFilter)
                return;

            Entity eventEntity = _eventQuery.GetSingletonEntity();

            DynamicBuffer<ChainLightningVisualEvent> events = _entityManager.GetBuffer<ChainLightningVisualEvent>(eventEntity);

            for (int i = 0; i < events.Length; i++)
            {
                ChainLightningVisualEvent visualEvent = events[i];

                PlayEvent(visualEvent);
            }

            events.Clear();
        }


        private void PlayEvent(ChainLightningVisualEvent visualEvent)
        {
            ChainLightningView view = _pool.Get();
            Vector3 from = new Vector3(visualEvent.From.x, visualEvent.From.y, _zPosition);
            Vector3 to = new Vector3(visualEvent.To.x, visualEvent.To.y, _zPosition);
            float delay = visualEvent.StepIndex * _jumpDelay;
            view.Play(from, to, delay, _lifetime, _width, _jitter, _pointCount, _flickerInterval);
        }


        private bool TryInitialize()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
                return false;

            if (_initialized && _world == world)
                return true;

            _world = world;
            _entityManager = world.EntityManager;
            
            _eventQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PresentationEventQueueTag>(),
                ComponentType.ReadWrite<ChainLightningVisualEvent>());
            
            _initialized = true;
            return true;
        }
    }
}