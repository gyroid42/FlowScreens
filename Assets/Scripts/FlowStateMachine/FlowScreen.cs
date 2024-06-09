using System.Collections.Generic;
using Collections;
using UnityEngine;

namespace FlowState
{
    public abstract class FlowScreen
    {
        private const int k_stateCapacity = 20;

        private struct FlowStateData
        {
            public FlowState State;
            public LifecycleState Lifecycle;
        }
        
        private enum Command
        {
            ADD_STATE,
            DISMISS_STATE,
            SET_ACTIVE_STATE,
            SET_INACTIVE_STATE
        }

        private struct FlowScreenTask
        {
            public Command Command;
            public FlowState State;
        }

        private readonly Queue<FlowScreenTask> m_taskQueue = new Queue<FlowScreenTask>();
        
        public LifecycleState CurrentLifecycleState { get; internal set; }
        
        private readonly SparseArray<FlowStateData> m_states = new SparseArray<FlowStateData>(k_stateCapacity);
        private readonly SparseArray<FlowState> m_activeStates = new SparseArray<FlowState>(k_stateCapacity);
        private readonly SparseArray<FlowState> m_inactiveStates = new SparseArray<FlowState>(k_stateCapacity);
        private readonly SparseArray<FlowState> m_dismissingStates = new SparseArray<FlowState>(k_stateCapacity);
        private readonly SparseArray<FlowState> m_presentingStates = new SparseArray<FlowState>(k_stateCapacity);
        
        public FlowStateMachine OwningFSM { get; internal set; }

        private Queue<(int flowStateKey, object message)> m_pendingMessageQueue;
        
        public void SendMessageActiveStates(object message)
        {
            for (var index = 0; index < m_activeStates.Length; index++)
            {
                var state = m_activeStates[index];
                m_pendingMessageQueue.Enqueue((state, message));
            }
        }

        public void ClearMessageQueue()
        {
            m_pendingMessageQueue.Clear();
        }

        public abstract void OnPresentingStart()
        {
        }

        public virtual FlowProgress OnPresentingUpdate()
        {
            return FlowProgress.COMPLETE;
        }

        public virtual void OnActiveStart()
        {
            for (var index = 0; index < m_activeStates.Length; index++)
            {
                var state = m_activeStates[index];
                state.OnActiveStart();
            }
        }

        public virtual void OnActiveUpdate()
        {
            for (var index = 0; index < m_presentingStates.Length; index++)
            {
                var state = m_presentingStates[index];
                FlowProgress progress = state.OnPresentingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    m_presentingStates.Remove(state.Key);
                    SetStateActive(state);
                    state.OnActiveStart();
                }
            }
            for (var index = 0; index < m_activeStates.Length; index++)
            {
                var state = m_activeStates[index];
                state.OnActiveUpdate();
            }
            for (var index = 0; index < m_inactiveStates.Length; index++)
            {
                var state = m_inactiveStates[index];
                state.OnInActiveUpdate();
            }
            for (var index = 0; index < m_dismissingStates.Length; index++)
            {
                var state = m_dismissingStates[index];
                FlowProgress progress = state.OnDismissingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    RemoveState(state);
                }
            }
        }

        public virtual void OnActiveLateUpdate()
        {
            for (var index = 0; index < m_activeStates.Length; index++)
            {
                var state = m_activeStates[index];
                state.OnActiveLateUpdate();
            }
        }

        public virtual void OnActiveFixedUpdate()
        {
            for (var index = 0; index < m_activeStates.Length; index++)
            {
                var state = m_activeStates[index];
                state.OnActiveFixedUpdate();
            }
            
        }

        public virtual void OnDismissingStart()
        {
            for (var index = 0; index < m_states.Length; index++)
            {
                var state = m_states[index];
                DismissState(state);
            }
        }

        public virtual FlowProgress OnDismissingUpdate()
        {
            for (var index = 0; index < m_dismissingStates.Length; index++)
            {
                var state = m_dismissingStates[index];
                if (state.OnDismissingUpdate() == FlowProgress.COMPLETE)
                {
                    RemoveState(state);
                }
            }
            
            return m_states.Length == 0? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }

        public virtual void OnDismissed()
        {
            
        }

        public virtual void OnInActiveStart()
        {
            
        }

        public virtual void OnInActiveUpdate()
        {
            
        }

        private void TryProcessNextFlowTask()
        {
            if (m_taskQueue.Count == 0)
            {
                return;
            }

            FlowScreenTask task = m_taskQueue.Dequeue();
            switch (task.Command)
            {
                case Command.ADD_STATE:
                {
                    AddState(task.State);
                    break;
                }

                case Command.DISMISS_STATE:
                {
                    DismissState(task.State);
                    break;
                }

                case Command.SET_ACTIVE_STATE:
                {
                    if (task.State.CurrentLifecycleState == LifecycleState.ACTIVE)
                    {
                        break;
                    }
                    m_inactiveStates.Remove(task.State.Key);
                    SetStateActive(task.State);
                    break;
                }

                case Command.SET_INACTIVE_STATE:
                {
                    if (task.State.CurrentLifecycleState == LifecycleState.INACTIVE)
                    {
                        break;
                    }
                    m_activeStates.Remove(task.State.Key);
                    SetStateInActive(task.State);
                    break;
                }
            }
        }

        private void DismissState(int key)
        {
            if (!m_states.TryGetValue(key, out var flowStateData))
            {
                return;
            }
            
            switch (flowStateData.Lifecycle)
            {
                case LifecycleState.ACTIVE:
                {
                    m_activeStates.Remove(key);
                    break;
                }

                case LifecycleState.INACTIVE:
                {
                    m_inactiveStates.Remove(key);
                    break;
                }

                case LifecycleState.PRESENTING:
                {
                    m_presentingStates.Remove(key);
                    break;
                }
            }

            m_dismissingStates.Insert(key, flowStateData.State);
            
            flowStateData.Lifecycle = LifecycleState.DISMISSING;
            flowStateData.State.OnDismissingStart();
        }

        private void RemoveState(int key)
        {
            var flowStateData = m_states.GetValue(key);
            flowStateData.State.OnDismissed();
            
            m_states.Remove(key);
            m_dismissingStates.Remove(key);
            
            // clear messages with flowstate
            flowState.ClearMessageQueue();
        }

        private void AddState(FlowState flowState)
        {
            if (m_states.Contains(state => state == flowState))
            {
                Debug.LogWarning();
                return;
            }
            
            flowState.CurrentLifecycleState = LifecycleState.PRESENTING;
            flowState.OwningScreen = this;
            
            flowState.Key = m_states.Insert(flowState);
            m_activeStates.Insert(flowState.Key, flowState);
            m_presentingStates.Insert(flowState.Key, flowState);
        }

        private void SetStateActive(FlowState flowState)
        {
            if (flowState.CurrentLifecycleState == LifecycleState.ACTIVE)
            {
                Debug.LogWarning();
                return;
            }

            Debug.Assert(m_states.Contains(flowState.Key));
            
            flowState.OnActiveStart();
            flowState.CurrentLifecycleState = LifecycleState.ACTIVE;

            m_activeStates.Insert(flowState.Key, flowState);
        }

        private void SetStateInActive(FlowState flowState)
        {
            if (flowState.CurrentLifecycleState == LifecycleState.INACTIVE)
            {
                Debug.LogWarning();
                return;
            }

            Debug.Assert(m_states.Contains(flowState.Key));
            
            flowState.OnInActiveStart();
            flowState.CurrentLifecycleState = LifecycleState.INACTIVE;

            m_inactiveStates.Insert(flowState.Key, flowState);
        }
    }
}