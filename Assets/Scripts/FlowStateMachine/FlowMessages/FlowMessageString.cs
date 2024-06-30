using UnityEngine;

namespace FlowState
{
    public sealed class FlowMessageString : FlowMessage
    {
        [SerializeField] private string m_message;

        public override object Message => m_message;
    }
}