
using Collections;
using UnityEngine;

namespace FlowState
{
    public class FlowStateMachine
    {
        private const byte k_stateStackCapacity = 32;
        private const byte k_commandCapacity = 32;
        private const short k_messageQueueCapacity = 32;
        private enum Command : byte
        {
            PUSH_STATE,
            POP_STATE
        }
        
        private FixedStackManaged<FlowState> m_stateStack = new FixedStackManaged<FlowState>(k_stateStackCapacity);

        private FixedQueueManaged<FlowState> m_flowStatesToAdd = new FixedQueueManaged<FlowState>(k_commandCapacity);
        private FixedQueue<Command> m_commandQueue = new FixedQueue<Command>(k_commandCapacity);
        
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
                        activeFlowState.LinkFlowGroups();
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
                    while (m_pendingMessageQueue.GetQueueCount(activeFlowState.Id) > 0)
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
                    
                    ProcessNextFlowCommand(activeFlowState);
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
                            ProcessNextFlowCommand(null);
                        }
                    }
                    break;
                }
            }
        }

        private void UpdateInActive()
        {
            for (int i = 0; i < m_stateStack.Count-1; i++)
            {
                m_stateStack[i].OnInActiveUpdate();
            }
        }
        
        private void ProcessNextFlowCommand(in FlowState activeFlowState)
        {
            Command command = m_commandQueue.Dequeue();
            switch (command)
            {
                case Command.PUSH_STATE:
                {
                    if (activeFlowState != null)
                    {
                        activeFlowState.CurrentLifecycleState = LifecycleState.INACTIVE;
                        activeFlowState.OnInActiveStart();
                    }

                    PushStateToStack(m_flowStatesToAdd.Dequeue());
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

        private void PushStateToStack(in FlowState flowState)
        {
            flowState.CurrentLifecycleState = LifecycleState.INITIALISING;
            flowState.OwningFSM = this;
            flowState.Id = (byte) m_stateStack.Count;

            m_stateStack.Push(flowState);
            
            flowState.OnInit();
        }
    }
}