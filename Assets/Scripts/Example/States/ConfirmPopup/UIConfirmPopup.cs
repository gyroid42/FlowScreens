using FlowStates.FlowMessages;
using UnityEngine;

namespace Example
{
    public class UIConfirmPopup : MonoBehaviour
    {
        [SerializeField] private FlowGroup flowGroup;
        
        public FlowGroup FlowGroup => flowGroup;
    }
}