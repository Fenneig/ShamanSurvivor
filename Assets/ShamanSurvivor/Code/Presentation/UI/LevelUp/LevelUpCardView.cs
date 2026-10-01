using System;
using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using ShamanSurvivor.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShamanSurvivor.Presentation
{
    public sealed class LevelUpCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private Image _frame;
        [SerializeField] private TMP_Text _abilityName;
        [SerializeField] private TMP_Text _keyName;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _progress;

        private int _index;

        private Action<int> _onSelected;

        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void BindAbility(int index, in LevelUpOption option, LevelUpCardCatalog catalog, Action<int> onSelected)
        {
            _index = index;
            _onSelected = onSelected;

            if (catalog.TryGetAbility(option.Ability, out var abilityVisual))
            {
                _abilityName.text = abilityVisual.DisplayName;
                _abilityName.color = abilityVisual.NameColor;
                _abilityIcon.sprite = abilityVisual.Icon;
                _frame.sprite = abilityVisual.Frame;
                _abilityIcon.enabled = abilityVisual.Icon != null;
            }
            else
            {
                _abilityName.text = option.Ability.ToString();
                _abilityIcon.enabled = false;
            }

            if (catalog.TryGetKey(option.Key, out var keyVisual))
            {
                _keyName.text = keyVisual.DisplayName;
                _description.text = string.Format(keyVisual.DescriptionFormat, option.Bonus);
            }
            else
            {
                _keyName.text = option.Key.ToString();
                _description.text = FormatBonus(option.Key, option.Bonus);
            }

            _progress.gameObject.SetActive(true);
            _progress.text = $"{option.CurrentPicks + 1}/{option.MaxPicks}";

            SetInteractable(true);

            gameObject.SetActive(true);
        }
        
        public void BindPassive(int index, in LevelUpOption option, LevelUpCardCatalog catalog, Action<int> onSelected)
        {
            _index = index;
            _onSelected = onSelected;

            if (catalog.TryGetPassive(option.Passive, out var passiveVisual))
            {
                _abilityName.text = passiveVisual.DisplayName;
                _keyName.text = passiveVisual.KeyName;
                _abilityName.color = passiveVisual.NameColor;
                _abilityIcon.sprite = passiveVisual.Icon;
                _frame.sprite = passiveVisual.Frame;
                _abilityIcon.enabled = passiveVisual.Icon != null;
                _description.text = string.Format(passiveVisual.DescriptionFormat, option.Bonus);
            }
            else
            {
                _abilityName.text = option.Ability.ToString();
                _abilityIcon.enabled = false;
            }

            _progress.gameObject.SetActive(true);
            _progress.text = $"{option.CurrentPicks + 1}/{option.MaxPicks}";

            SetInteractable(true);

            gameObject.SetActive(true);
        }

        public void BindUnlockAbility(int index, LevelUpOption option, LevelUpCardCatalog catalog, Action<int> onSelected)
        {
            _index = index;
            _onSelected = onSelected;

            if (catalog.TryGetUnlockAbility(option.Ability, out var unlockAbilityVisual))
            {
                _abilityName.text = unlockAbilityVisual.DisplayName;
                _keyName.text = unlockAbilityVisual.DisplayName;
                _abilityName.color = unlockAbilityVisual.NameColor;
                _abilityIcon.sprite = unlockAbilityVisual.Icon;
                _frame.sprite = unlockAbilityVisual.Frame;
                _abilityIcon.enabled = unlockAbilityVisual.Icon != null;
                _description.text = string.Format(unlockAbilityVisual.DescriptionFormat, option.Bonus);
            }
            else
            {
                _abilityName.text = option.Ability.ToString();
                _abilityIcon.enabled = false;
            }

            _progress.gameObject.SetActive(false);
            
            SetInteractable(true);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(_index);
        }

        private static string FormatBonus(UpgradeKey key, float value)
        {
            return key switch
            {
                UpgradeKey.Quantity => $"+{(int)value}",
                _ => $"+{value:P0}"
            };
        }
    }
}