using UnityEngine;
using UnityEngine.UI;

namespace ShamanSurvivor.Presentation
{
    public sealed class AbilityHudSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _cooldownFill;
        
        public bool Inited { get; private set; }

        public void Bind(Sprite icon)
        {
            Inited = true;
            _icon.sprite = icon;
            _icon.enabled = true;
            _cooldownFill.enabled = true;
            _cooldownFill.fillAmount = 0f;
        }

        public void SetCooldown(float remaining, float duration)
        {
            if (duration <= 0f || remaining <= 0f)
            {
                _cooldownFill.fillAmount = 0f;

                return;
            }

            _cooldownFill.fillAmount = Mathf.Clamp01(remaining / duration);
        }
    }
}