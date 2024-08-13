using FlowStates.FlowMessageUnion;
using UnityEngine;

namespace FlowStates.FlowMessages
{
    public abstract class FlowMessage : MonoBehaviour
    {
        public abstract FlowMessageData Message { get; }
    }
}