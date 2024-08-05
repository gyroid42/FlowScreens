using UnityEngine;

namespace FlowState
{
    public class FlowGroup : MonoBehaviour
    {
        private FlowStateMachine m_fsm;
        private byte m_flowStateId;
        private short m_flowWindowId;

        public void Link(FlowStateMachine fsm, byte flowStateId, short flowWindowId = -1)
        {
            m_fsm = fsm;
            m_flowStateId = flowStateId;
            m_flowWindowId = flowWindowId;
        }

        public void SendFlowMessage(object message)
        {
            m_fsm.SendMessageToState(m_flowStateId, m_flowWindowId, message);
        }
    }
}