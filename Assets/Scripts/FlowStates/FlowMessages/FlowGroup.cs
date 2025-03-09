using FlowStates.FlowMessageUnion;
using UnityEngine;

namespace FlowStates.FlowMessages
{
    public class FlowGroup : MonoBehaviour
    {
        private FlowStateMachine m_fsm;
        private int m_flowStateId;
        private byte m_flowWindowId;

        public void Link(FlowStateMachine fsm, int flowStateId, byte flowWindowId = byte.MaxValue)
        {
            m_fsm = fsm;
            m_flowStateId = flowStateId;
            m_flowWindowId = flowWindowId;
        }

        public void SendFlowMessage(FlowMessageData message)
        {
            m_fsm.SendMessageToStateIfActive(message, m_flowStateId, m_flowWindowId);
        }
    }
}