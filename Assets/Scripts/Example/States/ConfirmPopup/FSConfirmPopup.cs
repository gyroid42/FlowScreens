using System.Threading.Tasks;
using FlowStates;
using FlowStates.FlowMessageUnion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Example
{
    public class FSConfirmPopup : FlowState
    {
        private const string k_uiPrefabAddress = "UIConfirmPopup.prefab";
        
        private UIConfirmPopup m_ui;
        private readonly int m_targetFlowStateId;
        private readonly byte m_targetWindowId;

        public FSConfirmPopup(FlowStateContext context, int targetFlowStateId, byte targetWindowId = byte.MaxValue) : base(context)
        {
            m_targetFlowStateId = targetFlowStateId;
            m_targetWindowId = targetWindowId;
        }

        protected override Task OnInit()
        {
            return LoadAssets();
        }
        
        private async Task LoadAssets()
        {
            var uiLoadHandle = Addressables.LoadAssetAsync<GameObject>(k_uiPrefabAddress);
            await uiLoadHandle.Task;

            var uiGo = Object.Instantiate(uiLoadHandle.Result, Context.UIContainer);
            m_ui = uiGo.GetComponentInChildren<UIConfirmPopup>();
        }

        internal override void LinkFlowGroups()
        {
            m_ui.FlowGroup.Link(OwningFSM, Id);
        }
        
        internal override void OnFlowMessageReceived(in FlowMessageData message)
        {
            switch (message.Field)
            {
                case FlowMessageType.CONFIRM_POPUP:
                {
                    OwningFSM.PopState();
                    OwningFSM.SendMessageToState(message, m_targetFlowStateId, m_targetWindowId);
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