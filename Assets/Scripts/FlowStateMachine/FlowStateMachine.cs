
using System.Collections.Generic;
using UnityEngine;

namespace FlowState
{
    public class FlowStateMachine
    {
        private const int k_stateStackCapacity = 32;
        private enum Command
        {
            PUSH_STATE,
            POP_STATE
        }

        private struct FlowCommand
        {
            public Command Command;
            public FlowState FlowState;
        }
        
        private readonly Stack<FlowState> m_stateStack = new Stack<FlowState>(k_stateStackCapacity);
        private readonly Queue<FlowCommand> m_commandQueue = new Queue<FlowCommand>();
        private readonly Queue<object>[] m_pendingMessageQueue = new Queue<object>[k_stateStackCapacity];

        private FlowState ActiveFlowState => m_stateStack.Count == 0? null : m_stateStack.Peek();

        public FlowStateMachine()
        {
            for (int i = 0; i < m_pendingMessageQueue.Length; i++)
            {
                m_pendingMessageQueue[i] = new Queue<object>();
            }
        }

        #region Public API

        public void SendMessageToActiveState(object message)
        {
            if (m_stateStack.Count <= 0)
            {
                Debug.LogWarning("attempting to send message to active state in FSM that is empty");
                return;
            }

            m_pendingMessageQueue[m_stateStack.Count].Enqueue(message);
        }
        
        public void SendMessageToState(int stateId, object message)
        {
            if (stateId >= m_stateStack.Count ||
                stateId < 0)
            {
                return;
            }

            m_pendingMessageQueue[stateId].Enqueue(message);
        }
        
        public void PushState(FlowState flowState)
        {
            m_commandQueue.Enqueue(new FlowCommand
            {
                Command = Command.PUSH_STATE, 
                FlowState = flowState, 
            });
        }

        public void PopState()
        {
            m_commandQueue.Enqueue(new FlowCommand { Command = Command.POP_STATE });
        }

        public void Update()
        {
            UpdateActive();
            UpdateInActive();
        }
        
        public void FixedUpdate()
        {
            FlowState activeFlowState = ActiveFlowState;

            if (!(activeFlowState is { CurrentLifecycleState: LifecycleState.ACTIVE }))
            {
                return;
            }

            activeFlowState.OnActiveFixedUpdateInternal();
        }
        
        public void LateUpdate()
        {
            FlowState activeFlowState = ActiveFlowState;

            if (!(activeFlowState is { CurrentLifecycleState: LifecycleState.ACTIVE }))
            {
                return;
            }

            activeFlowState.OnActiveLateUpdateInternal();
        }

        #endregion
        

        
        private void UpdateActive()
        {
            FlowState activeFlowState = ActiveFlowState;
            if (activeFlowState == null)
            {
                return;
            }

            switch (activeFlowState.CurrentLifecycleState)
            {
                case LifecycleState.PRESENTING:
                {
                    FlowProgress presentingProgress = activeFlowState.OnPresentingUpdate();
                    if (presentingProgress == FlowProgress.COMPLETE)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.ACTIVE;
                        activeFlowState.OnActiveStartInternal();
                    }
                    break;
                }

                case LifecycleState.ACTIVE:
                {
                    while (m_pendingMessageQueue[activeFlowState.Id].Count > 0)
                    {
                        var message = m_pendingMessageQueue[activeFlowState.Id].Dequeue();
                        activeFlowState.OnFlowMessageReceived(message);
                    }

                    ProcessNextFlowCommand(m_stateStack, m_commandQueue, activeFlowState);
                    
                    activeFlowState.OnActiveUpdateInternal();
                    break;
                }

                case LifecycleState.DISMISSING:
                {
                    FlowProgress dismissProgress = activeFlowState.OnDismissingUpdateInternal();
                    if (dismissProgress == FlowProgress.COMPLETE)
                    {
                        m_stateStack.Pop();
                        activeFlowState.OnDismissed();
                        activeFlowState.CurrentLifecycleState = LifecycleState.DISMISSED;
                        activeFlowState.ClearMessageQueue();
                        m_pendingMessageQueue[activeFlowState.Id].Clear();

                        if (m_stateStack.Count > 0)
                        {
                            activeFlowState = m_stateStack.Peek();
                            activeFlowState.CurrentLifecycleState = LifecycleState.ACTIVE;
                            activeFlowState.OnActiveStartInternal();
                        }
                        else if (m_commandQueue.Count > 0)
                        {
                            ProcessNextFlowCommand(m_stateStack, m_commandQueue, null);
                        }
                    }
                    break;
                }
            }
        }

        private void UpdateInActive()
        {
            int index = 0;
            foreach (var state in m_stateStack)
            {
                if (index >= m_stateStack.Count-1)
                {
                    break;
                }
                
                state.OnInActiveUpdate();

                index++;
            }
        }
        
        private void ProcessNextFlowCommand(in Stack<FlowState> stateStack, in Queue<FlowCommand> commandQueue, in FlowState activeFlowState)
        {
            FlowCommand command = commandQueue.Dequeue();
            switch (command.Command)
            {
                case Command.PUSH_STATE:
                {
                    if (activeFlowState != null)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.INACTIVE;
                        activeFlowState.OnInActiveStart();
                    }

                    PushStateToStack(stateStack, command.FlowState);
                    break;
                }
                    
                case Command.POP_STATE:
                {
                    if (activeFlowState == null)
                    {
                        break;
                    }

                    activeFlowState.CurrentLifecycleState = LifecycleState.DISMISSING;
                    activeFlowState.OnDismissingStartInternal();
                    break;
                }
            }
        }

        private void PushStateToStack(in Stack<FlowState> stateStack, in FlowState flowState)
        {
            flowState.CurrentLifecycleState = LifecycleState.PRESENTING;
            flowState.OwningFSM = this;
            flowState.Id = stateStack.Count;

            stateStack.Push(flowState);

            flowState.OnPresentingStart();
        }
    }
}