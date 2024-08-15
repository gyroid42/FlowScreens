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
        
        private Task<GameObject> m_initTask;
        private UIConfirmPopup m_ui;
        private readonly byte m_targetFlowStateId;
        private readonly byte m_targetWindowId;

        public FSConfirmPopup(FlowStateContext context, byte targetFlowStateId, byte targetWindowId = byte.MaxValue) : base(context)
        {
            m_targetFlowStateId = targetFlowStateId;
            m_targetWindowId = targetWindowId;
        }

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
            m_ui = uiGo.GetComponentInChildren<UIConfirmPopup>();

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