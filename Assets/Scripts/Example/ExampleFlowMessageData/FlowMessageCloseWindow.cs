using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public class FlowMessageCloseWindow : FlowMessage
    {
        [SerializeField] private FlowMessageDataCloseWindow message;

        public override FlowMessageData Message => new FlowMessageData
        {
            CloseWindow = message
        };
    }
}