using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public sealed class FlowMessageSetAnimationSpeed : FlowMessage
    {
        [SerializeField] private FlowMessageDataSetAnimationSpeed message;

        public override FlowMessageData Message => new FlowMessageData
        {
            SetAnimationSpeed = message
        };
    }
}