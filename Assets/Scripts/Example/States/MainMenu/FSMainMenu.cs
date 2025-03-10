﻿using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSMainMenu : FlowState
    {
        private const string k_uiPrefabAddress = "UIMainMenu.prefab";
        
        private UIMainMenu m_ui;

        public FSMainMenu(FlowStateContext context) : base(context) { }
        
        protected override Task OnInit()
        {
            return LoadAssets();
        }
        
        private async Task LoadAssets()
        {
            var uiLoadHandle = Addressables.LoadAssetAsync<GameObject>(k_uiPrefabAddress);
            await uiLoadHandle.Task;

            var uiGo = Object.Instantiate(uiLoadHandle.Result, Context.UIContainer);
            m_ui = uiGo.GetComponentInChildren<UIMainMenu>();
        }

        internal override void LinkFlowGroups()
        {
            m_ui.FlowGroup.Link(OwningFSM, Id);
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
            m_ui.OnActiveUpdate(Time.deltaTime * Context.AnimationSpeed);
        }

        protected override void OnDismissingStart()
        {
            m_ui.OnDismissStart();
        }

        internal override void OnDismissed()
        {
            Object.Destroy(m_ui.gameObject);
        }

        protected override FlowProgress OnDismissingUpdate()
        {
            return m_ui.OnDismissingUpdate(Time.deltaTime * Context.AnimationSpeed) ? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        private void HandleMenuNavigationMessage(in FlowMessageDataMenuNavigation message)
        {
            switch (message.navigation)
            {
                case MenuNavigation.SCREEN_1:
                {
                    OwningFSM.PopState();
                    OwningFSM.PushState(new FSScreen1(Context));
                    break;
                }

                case MenuNavigation.BACK:
                case MenuNavigation.QUIT:
                {
                    Application.Quit();
                    return;
                }
            }
        }

        private void HandleSetAnimationSpeedMessage(in FlowMessageDataSetAnimationSpeed message)
        {
            Context.AnimationSpeed = message.speed;
        }
    }
}