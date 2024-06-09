
using System.Collections.Generic;
using UnityEngine;

namespace FlowState
{
    public class FlowStateMachine
    {
        private enum Command
        {
            PUSH_SCREEN,
            POP_SCREEN
        }

        private struct FlowTask
        {
            public Command Command;
            public FlowScreen FlowScreen;
        }
        
        private readonly Stack<FlowScreen> m_screenStack = new Stack<FlowScreen>();
        private readonly Queue<FlowTask> m_taskQueue = new Queue<FlowTask>();
        private readonly Queue<object>[] m_pendingMessageQueue = new Queue<object>[32];

        private FlowScreen ActiveFlowScreen => m_screenStack.Count == 0? null : m_screenStack.Peek();
        
        public void PushScreen(FlowScreen flowScreen)
        {
            m_taskQueue.Enqueue(new FlowTask
            {
                Command = Command.PUSH_SCREEN, 
                FlowScreen = flowScreen, 
            });
        }

        public void PopScreen()
        {
            m_taskQueue.Enqueue(new FlowTask { Command = Command.POP_SCREEN });
        }
        
        public void SendFlowMessageToActiveStates(object message)
        {
            var activeScreen = ActiveFlowScreen;

            if (activeScreen == null)
            {
                Debug.LogWarning($"trying to send message {message} to empty flowScreen stack");
                return;
            }

            activeScreen.SendMessageActiveStates(message);
        }

        public void Update()
        {
            FlowScreen activeFlowScreen = ActiveFlowScreen;
            if (activeFlowScreen == null)
            {
                return;
            }

            switch (activeFlowScreen.CurrentLifecycleState)
            {
                case LifecycleState.PRESENTING:
                {
                    FlowProgress presentingProgress = activeFlowScreen.OnPresentingUpdate();
                    if (presentingProgress == FlowProgress.COMPLETE)
                    {
                        activeFlowScreen.CurrentLifecycleState = LifecycleState.ACTIVE;
                        activeFlowScreen.OnActiveStart();
                    }
                    break;
                }

                case LifecycleState.ACTIVE:
                {
                    activeFlowScreen.OnActiveUpdate();
                    break;
                }

                case LifecycleState.DISMISSING:
                {
                    FlowProgress dismissProgress = activeFlowScreen.OnDismissingUpdate();
                    if (dismissProgress == FlowProgress.COMPLETE)
                    {
                        m_screenStack.Pop();
                        activeFlowScreen.OnDismissed();
                        activeFlowScreen.CurrentLifecycleState = LifecycleState.DISMISSED;
                        activeFlowScreen.ClearMessageQueue();

                        if (m_screenStack.Count > 0)
                        {
                            activeFlowScreen = m_screenStack.Peek();
                            activeFlowScreen.CurrentLifecycleState = LifecycleState.ACTIVE;
                            activeFlowScreen.OnActiveStart();
                        }
                    }
                    break;
                }
            }
        }

        public void InActiveUpdate()
        {
            var stackArray = m_screenStack.ToArray();

            for (int i = 1; i < stackArray.Length; i++)
            {
                stackArray[i].OnInActiveUpdate();
            }
        }

        public void FixedUpdate()
        {
            FlowScreen activeFlowScreen = ActiveFlowScreen;

            if (activeFlowScreen == null || activeFlowScreen.CurrentLifecycleState != LifecycleState.ACTIVE)
            {
                return;
            }

            activeFlowScreen.OnActiveFixedUpdate();
        }
        
        public void LateUpdate()
        {
            FlowScreen activeFlowScreen = ActiveFlowScreen;

            if (!(activeFlowScreen is { CurrentLifecycleState: LifecycleState.ACTIVE }))
            {
                return;
            }

            activeFlowScreen.OnActiveLateUpdate();
        }

        private void TryProcessNextFlowTask(FlowScreen activeFlowScreen)
        {
            if (m_taskQueue.Count <= 0)
            {
                return;
            }
            
            FlowTask task = m_taskQueue.Dequeue();
            switch (task.Command)
            {
                case Command.PUSH_SCREEN:
                {
                    if (activeFlowScreen != null)
                    {
                        activeFlowScreen.CurrentLifecycleState = LifecycleState.INACTIVE;
                        activeFlowScreen.OnInActiveStart();
                    }

                    PushScreenToStack(task.FlowScreen);
                    break;
                }
                    
                case Command.POP_SCREEN:
                {
                    if (activeFlowScreen == null)
                    {
                        break;
                    }

                    activeFlowScreen.CurrentLifecycleState = LifecycleState.DISMISSING;
                    activeFlowScreen.OnDismissingStart();
                    break;
                }
            }
        }

        private void PushScreenToStack(FlowScreen flowScreen)
        {
            flowScreen.CurrentLifecycleState = LifecycleState.PRESENTING;
            flowScreen.OwningFSM = this;

            m_screenStack.Push(flowScreen);

            flowScreen.OnPresentingStart();
        }
    }
}