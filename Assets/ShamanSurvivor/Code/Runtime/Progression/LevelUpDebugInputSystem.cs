using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameMetaSystemGroup))]
    [UpdateAfter(typeof(LevelUpFlowSystem))]
    public partial class LevelUpDebugInputSystem : SystemBase
    {
        private bool _wasOpen;

        protected override void OnCreate()
        {
            RequireForUpdate<GameFlowState>();
        }

        protected override void OnUpdate()
        {
            Entity progressionEntity = SystemAPI.GetSingletonEntity<GameFlowState>();

            GameFlowState flow = SystemAPI.GetComponent<GameFlowState>(progressionEntity);

            bool open = flow.Phase == GamePhase.LevelUp;

            DynamicBuffer<LevelUpOption> options = EntityManager.GetBuffer<LevelUpOption>(progressionEntity, true);

            if (open && !_wasOpen)
            {
                Debug.Log("=== LEVEL UP ===");

                for (int i = 0;
                     i < options.Length;
                     i++)
                {
                    LevelUpOption option = options[i];
                    Debug.Log($"{i + 1}: " + $"{option.Ability} + {option.Key} " + $"({option.CurrentPicks}/{option.MaxPicks})");
                }
            }

            _wasOpen = open;

            if (!open)
                return;

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            int selected = -1;

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                selected = 0;
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                selected = 1;
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                selected = 2;
            }

            if (selected < 0 || selected >= options.Length)
            {
                return;
            }

            LevelUpState state = EntityManager.GetComponentData<LevelUpState>(progressionEntity);

            state.SelectedIndex = selected;

            EntityManager.SetComponentData(progressionEntity, state);
        }
    }
}