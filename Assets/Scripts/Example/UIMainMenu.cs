using FlowStates.FlowMessages;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Example
{
    public class UIMainMenu : MonoBehaviour
    {
        public FlowGroup flowGroup;

        [SerializeField] private RectTransform imageTransform;
        [SerializeField] private float frequency;
        [SerializeField] private float amplitude;

        private float m_time = 0f;
        
        public void UpdateUI(float dt)
        {
            m_time += dt;

            float angle = amplitude * math.sin(m_time * frequency * math.PI * 2f);

            imageTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}