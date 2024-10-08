﻿using FlowStates.FlowMessages;
using Unity.Mathematics;
using UnityEngine;

namespace Example
{
    public class UIScreen2 : MonoBehaviour
    {
        [SerializeField] private FlowGroup flowGroup;
        [SerializeField] private RectTransform windowContainer;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float presentTime;
        [SerializeField] private float dismissTime;
        
        public FlowGroup FlowGroup => flowGroup;
        public RectTransform WindowContainer => windowContainer;

        private float m_time = 0f;

        public void Init()
        {
            canvasGroup.alpha = 0f;
        }
        
        public bool OnPresentingUpdate(float dt)
        {
            m_time += dt;

            canvasGroup.alpha = math.clamp(Easing.EaseOut(m_time / presentTime), 0f, 1f);

            return m_time >= presentTime;
        }

        public void OnDismissStart()
        {
            m_time = 0f;
        }

        public bool OnDismissingUpdate(float dt)
        {
            m_time += dt;
            
            canvasGroup.alpha = math.clamp(1f - Easing.EaseOut(math.clamp(m_time / dismissTime, 0f, 1f)), 0f, 1f);

            return m_time >= dismissTime;
        }
    }
}