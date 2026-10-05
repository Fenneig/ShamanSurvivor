using ShamanSurvivor.Runtime;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace ShamanSurvivor.Presentation
{
    public class PlayerHudPresenter : MonoBehaviour
    {
        [Header("Health")] 
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Image _healthFill;

        [Header("Experience")]
        [SerializeField] private Image _experienceFill;

        [SerializeField] private TMP_Text _experienceText;
        /*[Header("Abilities")] [SerializeField] private Transform _abilityContainer;
        [SerializeField] private AbilityHudSlot _abilitySlotPrefab;
        [SerializeField] private LevelUpCardCatalog _cardCatalog;
        private readonly Dictionary<AbilityId, AbilityHudSlot> _abilitySlots = new();
        */

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _playerQuery;
        private Entity _playerEntity = Entity.Null;
        private bool _initialized;

        private void Update()
        {
            if (!TryInitializeEcs())
                return;

            if (!TryResolvePlayer())
                return;

            UpdateHealth();
            UpdateExperience();
            //UpdateAbilities();
        }

        private bool TryInitializeEcs()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
                return false;

            if (_initialized && _world == world)
                return true;

            _world = world;

            _entityManager = world.EntityManager;

            _playerQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadOnly<PlayerExperience>(),
                ComponentType.ReadOnly<AbilityState>());

            _initialized = true;
            _playerEntity = Entity.Null;

            return true;
        }


        private bool TryResolvePlayer()
        {
            if (_playerEntity != Entity.Null && _entityManager.Exists(_playerEntity))
                return true;

            if (_playerQuery.IsEmptyIgnoreFilter)
            {
                _playerEntity = Entity.Null;

                return false;
            }

            _playerEntity = _playerQuery.GetSingletonEntity();

            return true;
        }


        private void UpdateHealth()
        {
            Health health = _entityManager.GetComponentData<Health>(_playerEntity);

            int current = Mathf.CeilToInt(Mathf.Max(0f, health.Current));
            int max = Mathf.CeilToInt(health.Max);
            
            float progress = current / (float)max;
            _healthFill.fillAmount = Mathf.Clamp01(progress);

            _healthText.text = $"{current} / {max}";
        }


        private void UpdateExperience()
        {
            PlayerExperience experience = _entityManager.GetComponentData<PlayerExperience>(_playerEntity);

            float progress = experience.Required > 0 ? experience.Current / (float)experience.Required : 0f;

            _experienceFill.fillAmount = Mathf.Clamp01(progress);

            if (_experienceText != null)
            {
                _experienceText.text = $"{experience.Current} / {experience.Required}";
            }
        }


        /*
        private void UpdateAbilities()
        {
            DynamicBuffer<AbilityState> abilities = _entityManager.GetBuffer<AbilityState>(_playerEntity, true);

            for (int i = 0; i < abilities.Length; i++)
            {
                AbilityState ability = abilities[i];
                AbilityHudSlot slot = GetOrCreateAbilitySlot(ability.Ability);
                slot.SetCooldown(ability.CooldownRemaining, ability.CooldownDuration);
            }
        }


        private AbilityHudSlot GetOrCreateAbilitySlot(AbilityId ability)
        {
            if (_abilitySlots.TryGetValue(ability, out AbilityHudSlot slot))
                return slot;

            slot = Instantiate(_abilitySlotPrefab, _abilityContainer);

            Sprite icon = null;

            if (_cardCatalog != null && _cardCatalog.TryGetAbility(ability, out var entry))
                icon = entry.Icon;

            slot.Bind(ability, icon);

            _abilitySlots.Add(ability, slot);

            return slot;
        }*/
    }
}