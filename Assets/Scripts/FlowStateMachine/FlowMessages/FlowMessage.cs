using FlowStateMachine.FlowMessageUnion;
using UnityEngine;

namespace FlowStateMachine.FlowMessages
{
    public abstract class FlowMessage : MonoBehaviour
    {
        public abstract FlowMessageData Message { get; }
    }
}