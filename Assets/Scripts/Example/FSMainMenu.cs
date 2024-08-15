﻿using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSMainMenu : FlowState
    {
        private const string k_uiGameAddress = "UIMainMenu.prefab";
        
        private Task<GameObject> m_initTask;
        private UIMainMenu m_ui;
        private float m_uiAnimationSpeed = 1f;

        public FSMainMenu(FlowStateContext context) : base(context) { }
        
        internal override void OnInit()
        {
            m_initTask = LoadAssets();
        }

        internal override FlowProgress OnInitUpdate()
        {
            if (!m_initTask.IsCompleted)
            {
                return FlowProgress.PROGRESSING;
            }

            var uiGo = Object.Instantiate(m_initTask.Result, Context.CanvasTransform);
            m_ui = uiGo.GetComponentInChildren<UIMainMenu>();

            m_initTask = null;
            
            return FlowProgress.COMPLETE;
        }

        private async Task<GameObject> LoadAssets()
        {
            var uiLoadHandle = Addressables.LoadAssetAsync<GameObject>(k_uiGameAddress);
            await uiLoadHandle.Task;

            return uiLoadHandle.Result;
        }

        internal override void LinkFlowGroups()
        {
            m_ui.flowGroup.Link(OwningFSM, Id);
        }

        internal override void OnFlowMessageReceived(in FlowMessageData message)
        {
            switch (message.Field)
            {
                case FlowMessageType.MENU_NAVIGATION:
                {
                    HandleMenuNavigationMessage(message.MenuNavigation);
                    break;
                }

                case FlowMessageType.SET_ANIMATION_SPEED:
                {
                    HandleSetAnimationSpeedMessage(message.SetAnimationSpeed);
                    break;
                }
            }
        }

        protected override void OnActiveUpdate()
        {
            m_ui.UpdateUI(Time.deltaTime * m_uiAnimationSpeed);
        }

        private void HandleMenuNavigationMessage(in FlowMessageDataMenuNavigation message)
        {
            switch (message.navigation)
            {
                case MenuNavigation.SCREEN_1:
                {
                    // push screen 1
                    break;
                }

                case MenuNavigation.SCREEN_2:
                {
                    // push screen 2
                    break;
                }

                case MenuNavigation.BACK:
                {
                    // close game
                    break;
                }
            }
        }

        private void HandleSetAnimationSpeedMessage(in FlowMessageDataSetAnimationSpeed message)
        {
            m_uiAnimationSpeed = message.speed;
        }
    }
}