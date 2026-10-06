using System.Collections.Generic;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class ChainLightningEffectPool : MonoBehaviour
    {
        [SerializeField] private ChainLightningView _prefab;

        [SerializeField]
        [Min(1)]
        private int _prewarmCount = 16;


        private readonly Queue<ChainLightningView>
            _available = new();


        private void Awake()
        {
            for (int i = 0; i < _prewarmCount; i++)
            {
                ChainLightningView view = CreateView();
                _available.Enqueue(view);
            }
        }


        public ChainLightningView Get()
        {
            if (_available.Count > 0)
                return _available.Dequeue();

            return CreateView();
        }


        public void Release(ChainLightningView view)
        {
            view.ResetView();
            _available.Enqueue(view);
        }


        public ChainLightningView CreateView()
        {
            ChainLightningView view = Instantiate(_prefab, transform);
            view.Initialize(this);
            view.gameObject.SetActive(false);
            return view;
        }
    }
}