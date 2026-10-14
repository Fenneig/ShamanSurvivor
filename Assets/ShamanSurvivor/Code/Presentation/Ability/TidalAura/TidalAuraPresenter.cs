using ShamanSurvivor.Runtime;
using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class TidalAuraPresenter : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private TidalAuraView _view;
        [Header("World")]
        [SerializeField] private float _zPosition = -0.15f;

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _playerQuery;
        private EntityQuery _catalogQuery;

        private Entity _player = Entity.Null;

        private void LateUpdate()
        {
            if (!TryInitialize())
            {
                Hide();
                return;
            }

            if (!TryGetPlayer())
            {
                Hide();
                return;
            }

            if (!TryGetAuraState(out AbilityState auraState))
            {
                Hide();
                return;
            }

            if (!TryGetAuraDefinition(out AbilityDefinition definition))
            {
                Hide();
                return;
            }

            LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_player);
            float sizeMultiplier = GetSizeMultiplier();
            float radius = definition.Range * sizeMultiplier;
            float cooldownPhase = GetCooldownPhase(auraState);
            float2 position = playerTransform.Position.xy;

            _view.Show(new Vector2(position.x, position.y), radius, cooldownPhase, _zPosition);
        }

        private bool TryInitialize()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
            {
                _world = null;
                _player = Entity.Null;
                return false;
            }

            if (_world == world)
                return true;

            _world = world;
            _entityManager = world.EntityManager;

            _playerQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                    ComponentType.ReadOnly<LocalTransform>(),
                    ComponentType.ReadOnly<AbilityState>(),
                    ComponentType.ReadOnly<UpgradeProgress>());

            _catalogQuery = _entityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<AbilityCatalogTag>(),
                    ComponentType.ReadOnly<AbilityDefinition>());

            _player = Entity.Null;

            return true;
        }

        private bool TryGetPlayer()
        {
            if (_player != Entity.Null && _entityManager.Exists(_player) && _entityManager.HasComponent<LocalTransform>(_player))
                return true;

            _player = Entity.Null;

            if (_playerQuery.IsEmptyIgnoreFilter)
                return false;

            _player = _playerQuery.GetSingletonEntity();

            return true;
        }

        private bool TryGetAuraState(out AbilityState result)
        {
            DynamicBuffer<AbilityState> abilities = _entityManager.GetBuffer<AbilityState>(_player, true);

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Ability != AbilityId.TidalAura)
                    continue;

                result = abilities[i];
                return true;
            }

            result = default;
            return false;
        }

        private bool TryGetAuraDefinition(out AbilityDefinition result)
        {
            result = default;

            if (_catalogQuery.IsEmptyIgnoreFilter)
                return false;

            Entity catalog = _catalogQuery.GetSingletonEntity();

            DynamicBuffer<AbilityDefinition> definitions = _entityManager.GetBuffer<AbilityDefinition>(catalog, true);

            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].Ability != AbilityId.TidalAura)
                    continue;

                result = definitions[i];
                return true;
            }

            return false;
        }

        private float GetSizeMultiplier()
        {
            DynamicBuffer<UpgradeProgress> upgrades = _entityManager.GetBuffer<UpgradeProgress>(_player, true);

            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeProgress upgrade = upgrades[i];

                if (upgrade.Ability != AbilityId.TidalAura || upgrade.Key != UpgradeKey.Size)
                    continue;

                return Mathf.Max(0f, 1f + upgrade.TotalBonus);
            }

            return 1f;
        }

        private float GetCooldownPhase(AbilityState ability)
        {
            if (ability.CooldownDuration <= 0f)
                return 0f;

            float remaining = Mathf.Clamp01(ability.CooldownRemaining / ability.CooldownDuration);

            return 1f - remaining;
        }

        private void Hide()
        {
            if (_view != null)
                _view.Hide();
        }

        private void OnDisable()
        {
            Hide();
        }
    }
}