using UnityEngine;
using UnityEngine.UI;

namespace FlowStates.FlowMessages
{
    [RequireComponent(typeof(Button))]
    public class FlowButton : MonoBehaviour
    {
        [SerializeField] private FlowMessage[] m_messages;

        private FlowGroup m_flowGroup;

        private void Awake()
        {
            m_flowGroup = GetComponentInParent<FlowGroup>();
            GetComponent<Button>().onClick.AddListener(ButtonPressed);
        }

        private void ButtonPressed()
        {
            foreach (var message in m_messages)
            {
                m_flowGroup.SendFlowMessage(message.Message);
            }
        }
    }
}