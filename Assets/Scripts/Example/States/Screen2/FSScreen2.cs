﻿using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSScreen2 : FlowState
    {
        private const string k_uiPrefabAddress = "UIScreen2.prefab";
        
        private Task<GameObject> m_initTask;
        private UIScreen2 m_ui;
        
        public FSScreen2(FlowStateContext context) : base(context) { }

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
            m_ui = uiGo.GetComponentInChildren<UIScreen2>();
            m_ui.Init();

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
                case MenuNavigation.BACK:
                {
                    OwningFSM.PopState();
                    break;
                }
            }
        }

        internal override FlowProgress OnPresentingUpdate()
        {
            return m_ui.OnPresentingUpdate(Time.deltaTime * Context.AnimationSpeed) ? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        protected override void OnDismissingStart()
        {
            m_ui.OnDismissStart();
        }

        protected override FlowProgress OnDismissingUpdate()
        {
            return m_ui.OnDismissingUpdate(Time.deltaTime * Context.AnimationSpeed)? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        internal override void OnDismissed()
        {
            Object.Destroy(m_ui.gameObject);
        }
    }
}