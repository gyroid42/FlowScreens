using FlowStates.FlowMessages;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    public class FlowMessageConfirmPopup : FlowMessage
    {
        [SerializeField] private FlowMessageDataConfirmPopup message;

        public override FlowMessageData Message => new FlowMessageData
        {
            ConfirmPopup = message
        };
    }
}