using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public sealed class FlowMessageOpenWindow : FlowMessage
    {
        [SerializeField] private FlowMessageDataOpenWindow message;

        public override FlowMessageData Message => new FlowMessageData
        {
            OpenWindow = message
        };
    }
}