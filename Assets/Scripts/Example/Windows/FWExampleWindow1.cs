using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example.Windows
{
    public class FWExampleWindow1 : FlowWindow
    {
        private const string k_uiPrefabAddress = "UIExampleWindow1.prefab";

        private Task<GameObject> m_initTask;
        private UIExampleWindow1 m_ui;
        
        public FWExampleWindow1(FlowStateContext context) : base(context) { }

        public override void OnInit()
        {
            m_initTask = LoadAssets();
        }
        
        public override FlowProgress OnInitUpdate()
        {
            if (!m_initTask.IsCompleted)
            {
                return FlowProgress.PROGRESSING;
            }

            var uiGo = Object.Instantiate(m_initTask.Result, Context.UIContainer);
            m_ui = uiGo.GetComponentInChildren<UIExampleWindow1>();
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

        public override void LinkFlowGroups(in int flowStateId)
        {
            m_ui.FlowGroup.Link(OwningFSM, flowStateId, Id);
        }

        public override void OnFlowMessageReceived(in FlowMessageData message)
        {
            switch (message.Field)
            {
                case FlowMessageType.MENU_NAVIGATION:
                {
                    HandleMenuNavigationMessage(message.MenuNavigation);
                    break;
                }

                case FlowMessageType.ALPHA_SELECT:
                {
                    m_ui.SetAlpha(message.AlphaSelect.alpha);
                    break;
                }
            }
        }
        
        private void HandleMenuNavigationMessage(in FlowMessageDataMenuNavigation message)
        {
            switch (message.navigation)
            {
                case MenuNavigation.BACK:
                case MenuNavigation.QUIT:
                {
                    OwningFSM.SendMessageToState(new FlowMessageData
                    {
                        CloseWindow = new FlowMessageDataCloseWindow()
                        {
                            window = WindowType.EXAMPLE_1
                        }
                    }, Owner.Id);
                    return;
                }
            }
        }

        public override FlowProgress OnPresentingUpdate()
        {
            return m_ui.OnPresentingUpdate(Time.deltaTime) ? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        public override void OnDismissingStart()
        {
            m_ui.OnDismissingStart();
        }

        public override FlowProgress OnDismissingUpdate()
        {
            return m_ui.OnDismissingUpdate(Time.deltaTime) ? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        public override void OnDismissed()
        {
            Object.Destroy(m_ui.gameObject);
        }
    }
}