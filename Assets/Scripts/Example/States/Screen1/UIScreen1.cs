using FlowStates.FlowMessages;
using UnityEngine;

namespace Example
{
    public class UIScreen1 : MonoBehaviour
    {
        [SerializeField] private FlowGroup flowGroup;
        
        public FlowGroup FlowGroup => flowGroup;
    }
}