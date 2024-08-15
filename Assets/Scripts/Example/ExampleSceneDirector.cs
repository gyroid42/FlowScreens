using FlowStates;
using UnityEngine;

namespace Example
{
    public class ExampleSceneDirector : MonoBehaviour
    {
        [SerializeField] private RectTransform uiCanvas;
        
        private readonly FlowStateMachine m_flowStateMachine = new FlowStateMachine();
        
        private void Start()
        {
            m_flowStateMachine.PushState(new FSGame(new FlowStateContext
            {
                CanvasTransform = uiCanvas
            }));
        }

        private void Update()
        {
            m_flowStateMachine.Update();
        }

        private void FixedUpdate()
        {
            m_flowStateMachine.FixedUpdate();
        }

        private void LateUpdate()
        {
            m_flowStateMachine.LateUpdate();
        }
    }
}