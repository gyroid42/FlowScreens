
using System.Collections.Generic;
using Collections;
using UnityEngine;

namespace FlowState
{
    public class FlowStateMachine
    {
        private const byte k_stateStackCapacity = 32;
        private const short k_messageQueueCapacity = 32;
        private enum Command : byte
        {
            PUSH_STATE,
            POP_STATE
        }
        
        private readonly Stack<FlowState> m_stateStack = new Stack<FlowState>(k_stateStackCapacity);

        private readonly Queue<FlowState> m_flowStatesToAdd = new Queue<FlowState>();
        private readonly Queue<Command> m_commandQueue = new Queue<Command>();
        
        private readonly QueueArray<(short windowId, FlowMessageData message)> m_pendingMessageQueue = new QueueArray<(short, FlowMessageData)>(k_stateStackCapacity, k_messageQueueCapacity);

        private FlowState ActiveFlowState => m_stateStack.Count == 0? null : m_stateStack.Peek();

        #region Public API

        public void SendMessageToActiveState(FlowMessageData message)
        {
            if (m_stateStack.Count <= 0)
            {
                Debug.LogWarning("attempting to send message to active state in FSM that is empty");
                return;
            }

            m_pendingMessageQueue.Enqueue(m_stateStack.Count -1, (-1, message));
        }
        
        public void SendMessageToState(byte stateId, short windowId, FlowMessageData message)
        {
            if (stateId >= m_stateStack.Count)
            {
                return;
            }

            m_pendingMessageQueue.Enqueue(stateId, (windowId, message));
        }
        
        public void PushState(FlowState flowState)
        {
            m_flowStatesToAdd.Enqueue(flowState);
            m_commandQueue.Enqueue(Command.PUSH_STATE);
        }

        public void PopState()
        {
            m_commandQueue.Enqueue(Command.POP_STATE);
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
                case LifecycleState.INITIALISING:
                {
                    if (activeFlowState.OnInitUpdate() == FlowProgress.COMPLETE)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.PRESENTING;
                        activeFlowState.OnPresentingStart();
                    }
                    break;
                }
                
                case LifecycleState.PRESENTING:
                {
                    if (activeFlowState.OnPresentingUpdate() == FlowProgress.COMPLETE)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.ACTIVE;
                        activeFlowState.OnActiveStartInternal();
                    }
                    break;
                }

                case LifecycleState.ACTIVE:
                {
                    while (m_pendingMessageQueue.Count(activeFlowState.Id) > 0)
                    {
                        var (window, message) = m_pendingMessageQueue.Dequeue(activeFlowState.Id);

                        if (window == -1)
                        {
                            activeFlowState.OnFlowMessageReceived(message);
                        }
                        else
                        {
                            activeFlowState.SendMessageToWindow(window, message);
                        }
                    }
                    
                    activeFlowState.OnActiveUpdateInternal();
                    
                    ProcessNextFlowCommand(m_stateStack, m_commandQueue, activeFlowState);
                    break;
                }

                case LifecycleState.DISMISSING:
                {
                    if (activeFlowState.OnDismissingUpdateInternal() == FlowProgress.COMPLETE)
                    {
                        m_stateStack.Pop();
                        activeFlowState.OnDismissed();
                        activeFlowState.CurrentLifecycleState = LifecycleState.DISMISSED;
                        m_pendingMessageQueue.Clear(activeFlowState.Id);

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
            int i = 0;
            foreach (var state in m_stateStack)
            {
                if (i >= m_stateStack.Count-1)
                {
                    break;
                }
                
                state.OnInActiveUpdate();

                i++;
            }
        }
        
        private void ProcessNextFlowCommand(in Stack<FlowState> stateStack, in Queue<Command> commandQueue, in FlowState activeFlowState)
        {
            Command command = commandQueue.Dequeue();
            switch (command)
            {
                case Command.PUSH_STATE:
                {
                    if (activeFlowState != null)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.INACTIVE;
                        activeFlowState.OnInActiveStart();
                    }

                    PushStateToStack(stateStack, m_flowStatesToAdd.Dequeue());
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
            flowState.CurrentLifecycleState = LifecycleState.INITIALISING;
            flowState.OwningFSM = this;
            flowState.Id = (byte) stateStack.Count;

            stateStack.Push(flowState);
            
            flowState.OnInit();
        }
    }
}