using FlowStates.FlowMessages;
using Unity.Mathematics;
using UnityEngine;

namespace Example.Windows
{
    public class UIExampleWindow1 : MonoBehaviour
    {
        [SerializeField] private FlowGroup flowGroup;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float presentTime;
        [SerializeField] private float dismissTime;
        
        public FlowGroup FlowGroup => flowGroup;

        private RectTransform m_rectTransform;
        private float m_time;

        public void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }
        
        public void Init()
        {
            m_rectTransform.anchoredPosition = new Vector2(-m_rectTransform.sizeDelta.x, 0f);
        }
        
        public bool OnPresentingUpdate(float dt)
        {
            m_time += dt;

            var sizeDelta = m_rectTransform.sizeDelta;
            var xPos = -sizeDelta.x + sizeDelta.x * math.clamp(m_time / presentTime, 0f, 1f);
            m_rectTransform.anchoredPosition = new Vector2(xPos, 0f);
            
            return m_time >= presentTime;
        }

        public void OnDismissingStart()
        {
            m_time = 0f;
        }

        public bool OnDismissingUpdate(float dt)
        {
            m_time += dt;

            var sizeDelta = m_rectTransform.sizeDelta;
            var xPos = -sizeDelta.x * math.clamp(m_time / presentTime, 0f, 1f);
            m_rectTransform.anchoredPosition = new Vector2(xPos, 0f);
            
            return m_time >= dismissTime;
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = math.clamp(alpha, 0f, 1f);
        }
    }
}