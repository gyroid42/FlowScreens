using UnityEngine;

namespace FlowState
{
    public abstract class FlowMessage : MonoBehaviour
    {
        public abstract FlowMessageData Message { get; }
    }
}