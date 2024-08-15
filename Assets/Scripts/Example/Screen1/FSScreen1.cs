﻿using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSScreen1 : FlowState
    {
        private const string k_uiPrefabAddress = "UIScreen1.prefab";

        private Task<GameObject> m_initTask;
        private UIScreen1 m_ui;
        
        public FSScreen1(FlowStateContext context) : base(context) { }

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
            m_ui = uiGo.GetComponentInChildren<UIScreen1>();

            m_initTask = null;
            
            return FlowProgress.COMPLETE;
        }
        
        private async Task<GameObject> LoadAssets()
        {
            var uiLoadHandle = Addressables.LoadAssetAsync<GameObject>(k_uiPrefabAddress);
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
            }
        }
        
        private void HandleMenuNavigationMessage(in FlowMessageDataMenuNavigation message)
        {
            switch (message.navigation)
            {
                case MenuNavigation.MAIN_MENU:
                {
                    OwningFSM.PopState();
                    OwningFSM.PushState(new FSMainMenu(Context));
                    break;
                }
            }
        }
        
        internal override void OnDismissed()
        {
            Object.Destroy(m_ui.gameObject);
        }
    }
}