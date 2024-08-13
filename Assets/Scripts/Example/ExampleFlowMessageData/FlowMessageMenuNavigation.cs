using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public class FlowMessageMenuNavigation : FlowMessage
    {
        [SerializeField] private FlowMessageDataMenuNavigation message;

        public override FlowMessageData Message => new FlowMessageData
        {
            MenuNavigation = message
        };
    }
}