using UnityEngine;

namespace FlowState
{
    public abstract class FlowMessage : MonoBehaviour
    {
        public abstract object Message { get; }
    }
}