using FlowStateMachine.FlowMessageUnion;
using UnityEngine;

namespace FlowStateMachine.FlowMessages
{
    public sealed class FlowMessageExample2 : FlowMessage
    {
        [SerializeField] private FlowMessageDataExample2 m_message;

        public override FlowMessageData Message
        {
            get
            {
                var message = new FlowMessageData
                {
                    Example2 = m_message
                };
                return message;
            }
        }
    }
}