using System.Threading.Tasks;
using Example.Windows;
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

        private byte m_windowIdExample1 = byte.MaxValue;
        
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

            var uiGo = Object.Instantiate(m_initTask.Result, Context.UIContainer);
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

                case FlowMessageType.OPEN_WINDOW:
                {
                    HandleOpenWindowMessage(message.OpenWindow.window);
                    break;
                }

                case FlowMessageType.CLOSE_WINDOW:
                {
                    HandleCloseWindowMessage(message.CloseWindow.window);
                    break;
                }
            }
        }

        private void HandleOpenWindowMessage(WindowType window)
        {
            switch (window)
            {
                case WindowType.EXAMPLE_1:
                {
                    if (m_windowIdExample1 != byte.MaxValue)
                    {
                        return;
                    }
                    
                    var context = Context;
                    context.UIContainer = m_ui.WindowContainer;
                    m_windowIdExample1 = AddWindow(new FWExampleWindow1(context));
                    break;
                }
            }
        }

        private void HandleCloseWindowMessage(WindowType window)
        {
            switch (window)
            {
                case WindowType.EXAMPLE_1:
                {
                    if (m_windowIdExample1 == byte.MaxValue)
                    {
                        return;
                    }
                    
                    DismissWindow(m_windowIdExample1);
                    m_windowIdExample1 = byte.MaxValue;
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