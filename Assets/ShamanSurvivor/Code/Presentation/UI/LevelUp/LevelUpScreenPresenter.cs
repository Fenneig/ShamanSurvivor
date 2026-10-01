using System;
using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class LevelUpScreenPresenter : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _root;

        [Header("Cards")]
        [SerializeField] private LevelUpCardView[] _cards;

        [Header("Data")]
        [SerializeField] private LevelUpCardCatalog _levelUpCardCatalog;

        private EntityManager _entityManager;
        private EntityQuery _query;
        private Entity _progressionEntity;
        private uint _lastRevision;
        private bool _visible;

        private void Start()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null)
            {
                enabled = false;
                return;
            }

            _entityManager = world.EntityManager;

            _query = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<GameFlowState>(), ComponentType.ReadWrite<LevelUpState>(), ComponentType.ReadOnly<LevelUpOption>());

            _root.SetActive(false);
        }

        private void Update()
        {
            if (!_progressionEntity.Equals(Entity.Null) &&
                !_entityManager.Exists(_progressionEntity))
            {
                _progressionEntity = Entity.Null;
            }

            if (_progressionEntity == Entity.Null)
            {
                if (_query.CalculateEntityCount() != 1)
                    return;

                _progressionEntity = _query.GetSingletonEntity();
            }

            GameFlowState flow = _entityManager.GetComponentData<GameFlowState>(_progressionEntity);

            if (flow.Phase != GamePhase.LevelUp)
            {
                Hide();

                return;
            }

            LevelUpState state = _entityManager.GetComponentData<LevelUpState>(_progressionEntity);

            if (!_visible || state.Revision != _lastRevision)
            {
                Refresh(state.Revision);
            }
        }

        private void Refresh(uint revision)
        {
            DynamicBuffer<LevelUpOption> options = _entityManager.GetBuffer<LevelUpOption>(_progressionEntity, true);

            _root.SetActive(true);
            _visible = true;
            _lastRevision = revision;

            for (int i = 0; i < _cards.Length; i++)
            {
                if (i >= options.Length)
                {
                    _cards[i].Hide();
                    continue;
                }

                LevelUpOption option = options[i];

                switch (option.Type)
                {
                    case LevelUpOptionType.AbilityUpgrade:
                        _cards[i].BindAbility(i, option, _levelUpCardCatalog, SelectOption);
                        break;
                    case LevelUpOptionType.GlobalPassive:
                        _cards[i].BindPassive(i, option, _levelUpCardCatalog, SelectOption);
                        break;
                    case LevelUpOptionType.UnlockAbility:
                        _cards[i].BindUnlockAbility(i, option, _levelUpCardCatalog, SelectOption);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SelectOption(int index)
        {
            if (!_visible)
                return;

            LevelUpState state = _entityManager.GetComponentData<LevelUpState>(_progressionEntity);

            if (state.SelectedIndex >= 0)
                return;

            state.SelectedIndex = index;

            _entityManager.SetComponentData(_progressionEntity, state);

            foreach (var card in _cards)
                card.SetInteractable(false);
        }

        private void Hide()
        {
            if (!_visible)
                return;

            _visible = false;

            _root.SetActive(false);
        }
    }
}