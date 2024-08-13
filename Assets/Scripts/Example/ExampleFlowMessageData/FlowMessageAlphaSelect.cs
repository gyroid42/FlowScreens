using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public sealed class FlowMessageAlphaSelect : FlowMessage
    {
        [SerializeField] private FlowMessageDataAlphaSelect message;

        public override FlowMessageData Message => new FlowMessageData
        {
            AlphaSelect = message
        };
    }
}