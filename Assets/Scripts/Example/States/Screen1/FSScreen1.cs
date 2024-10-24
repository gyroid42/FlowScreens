using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSScreen1 : FlowState
    {
        private const string k_uiPrefabAddress = "UIScreen1.prefab";

        private UIScreen1 m_ui;
        
        public FSScreen1(FlowStateContext context) : base(context) { }

        protected override Task OnInit()
        {
            return LoadAssets();
        }
        
        private async Task LoadAssets()
        {
            var uiLoadHandle = Addressables.LoadAssetAsync<GameObject>(k_uiPrefabAddress);
            await uiLoadHandle.Task;

            var uiGo = Object.Instantiate(uiLoadHandle.Result, Context.UIContainer);
            m_ui = uiGo.GetComponentInChildren<UIScreen1>();
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

                case FlowMessageType.TRY_DO_ACTION:
                {
                    OwningFSM.PushState(new FSConfirmPopup(Context, Id));
                    break;
                }

                case FlowMessageType.CONFIRM_POPUP:
                {
                    Debug.Log($"received confirm popup, confirmation = {message.ConfirmPopup.confirm}");
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
                
                case MenuNavigation.SCREEN_2:
                {
                    OwningFSM.PushState(new FSScreen2(Context));
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