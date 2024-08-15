using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public sealed class FlowMessageTryDoAction : FlowMessage
    {
        [SerializeField] private FlowMessageDataTryDoAction message;

        public override FlowMessageData Message => new FlowMessageData
        {
            TryDoAction = message
        };
    }
}