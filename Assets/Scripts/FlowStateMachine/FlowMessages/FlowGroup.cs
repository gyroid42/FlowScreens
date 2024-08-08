using UnityEngine;

namespace FlowState
{
    public class FlowGroup : MonoBehaviour
    {
        private FlowStateMachine m_fsm;
        private byte m_flowStateId;

        public void Link(FlowStateMachine fsm, byte flowStateId)
        {
            m_fsm = fsm;
            m_flowStateId = flowStateId;
        }

        public void SendFlowMessage(FlowMessageData message)
        {
            m_fsm.SendMessageToState(m_flowStateId, message);
        }
    }
}