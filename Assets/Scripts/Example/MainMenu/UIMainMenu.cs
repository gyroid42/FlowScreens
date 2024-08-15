using FlowStates.FlowMessages;
using Unity.Mathematics;
using UnityEngine;

namespace Example
{
    public class UIMainMenu : MonoBehaviour
    {
        public FlowGroup flowGroup;

        [SerializeField] private RectTransform imageTransform;
        [SerializeField] private float frequency;
        [SerializeField] private float amplitude;

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private float dismissTime;

        private float m_time = 0f;
        
        public void UpdateUI(float dt)
        {
            m_time += dt;

            float angle = amplitude * math.sin(m_time * frequency * math.PI * 2f);
            imageTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void OnDismissStart()
        {
            m_time = 0f;
        }

        public bool UpdateDismissing(float dt)
        {
            m_time += dt;
            canvasGroup.alpha = math.clamp(1f - Easing.EaseIn(m_time / dismissTime), 0f, 1f);

            return m_time >= dismissTime;
        }
    }
}