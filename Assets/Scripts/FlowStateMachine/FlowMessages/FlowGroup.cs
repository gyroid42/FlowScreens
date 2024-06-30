using UnityEngine;

namespace FlowState
{
    public class FlowGroup : MonoBehaviour
    {
        private FlowStateMachine m_fsm;
        private int m_flowStateId;
        private int m_flowWindowId;

        public void Link(FlowStateMachine fsm, int flowStateId, int flowWindowId = -1)
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