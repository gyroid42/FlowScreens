using System.Collections.Generic;
using Collections;
using UnityEngine;

namespace FlowState
{
    public abstract class FlowState
    {
        private const int k_windowCapacity = 20;
        
        private enum Command
        {
            ADD_WINDOW,
            DISMISS_WINDOW,
            SET_ACTIVE_STATE,
            SET_INACTIVE_STATE
        }

        private struct FlowWindowCommand
        {
            public Command Command;
            public FlowWindow Window;
        }
        
        private readonly SparseArray<FlowWindow> m_windows = new SparseArray<FlowWindow>(k_windowCapacity);
        private readonly SparseArray<FlowWindow> m_activeWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private readonly SparseArray<FlowWindow> m_inactiveWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private readonly SparseArray<FlowWindow> m_dismissingWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private readonly SparseArray<FlowWindow> m_presentingWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        
        private readonly Queue<FlowWindowCommand> m_commandQueue = new Queue<FlowWindowCommand>();
        
        public FlowStateMachine OwningFSM { get; internal set; }
        public LifecycleState CurrentLifecycleState { get; internal set; }
        public int Id { get; internal set; }
        
        
        #region Internal API
        
        internal void SendMessageToWindow(int windowId, object message)
        {
            if (m_windows.TryGetValue(windowId, out var window))
            {
                window.OnFlowMessageReceived(message);
            }
        }
        
                internal void OnActiveStartInternal()
        {
            for (var index = 0; index < m_activeWindows.Length; index++)
            {
                var window = m_activeWindows[index];
                window.OnActiveStart();
            }
            
            OnActiveStart();
        }
        
        internal void OnActiveUpdateInternal()
        {
            while (m_commandQueue.Count > 0)
            {
                ProcessNextWindowCommand();
            }

            for (var index = 0; index < m_presentingWindows.Length; index++)
            {
                var window = m_presentingWindows[index];
                FlowProgress progress = window.OnPresentingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    m_presentingWindows.Remove(window.Id);
                    SetWindowActive(window);
                    window.OnActiveStart();
                }
            }
            for (var index = 0; index < m_activeWindows.Length; index++)
            {
                var window = m_activeWindows[index];
                window.OnActiveUpdate();
            }
            for (var index = 0; index < m_inactiveWindows.Length; index++)
            {
                var window = m_inactiveWindows[index];
                window.OnInActiveUpdate();
            }
            for (var index = 0; index < m_dismissingWindows.Length; index++)
            {
                var window = m_dismissingWindows[index];
                FlowProgress progress = window.OnDismissingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    RemoveWindow(window.Id);
                }
            }
            
            OnActiveUpdate();
        }
        
        internal void OnActiveLateUpdateInternal()
        {
            for (var index = 0; index < m_activeWindows.Length; index++)
            {
                var window = m_activeWindows[index];
                window.OnActiveLateUpdate();
            }
            
            OnActiveLateUpdate();
        }

        internal void OnActiveFixedUpdateInternal()
        {
            for (var index = 0; index < m_activeWindows.Length; index++)
            {
                var window = m_activeWindows[index];
                window.OnActiveFixedUpdate();
            }
            
            OnActiveFixedUpdate();
        }

        internal void OnDismissingStartInternal()
        {
            for (var index = 0; index < m_windows.Length; index++)
            {
                var window = m_windows[index];
                DismissState(window.Id);
            }
            
            OnDismissingStart();
        }

        internal FlowProgress OnDismissingUpdateInternal()
        {
            for (var index = 0; index < m_dismissingWindows.Length; index++)
            {
                var window = m_dismissingWindows[index];
                if (window.OnDismissingUpdate() == FlowProgress.COMPLETE)
                {
                    RemoveWindow(window.Id);
                }
            }

            if (OnDismissingUpdate() == FlowProgress.PROGRESSING)
            {
                return FlowProgress.PROGRESSING;
            }
            
            return m_windows.Length == 0? FlowProgress.COMPLETE : FlowProgress.PROGRESSING;
        }
        

        #endregion
        
        #region Virtual Methods

        internal virtual void OnFlowMessageReceived(object message) { }

        internal virtual void OnInit() {  }
        internal virtual FlowProgress OnInitUpdate() => FlowProgress.COMPLETE;
        
        
        internal virtual void OnPresentingStart() { }
        internal virtual FlowProgress OnPresentingUpdate() => FlowProgress.COMPLETE;
        
        
        protected virtual void OnActiveStart() { }
        protected virtual void OnActiveUpdate() { }
        protected virtual void OnActiveLateUpdate() { }
        protected virtual void OnActiveFixedUpdate() { }
        
        
        protected virtual void OnDismissingStart() { }
        protected virtual FlowProgress OnDismissingUpdate() => FlowProgress.COMPLETE;
        internal virtual void OnDismissed() { }
        
        
        internal virtual void OnInActiveStart() { }
        internal virtual void OnInActiveUpdate() { }

        #endregion
        
        #region Private Methods
        
        private void ProcessNextWindowCommand()
        {
            FlowWindowCommand command = m_commandQueue.Dequeue();
            switch (command.Command)
            {
                case Command.ADD_WINDOW:
                {
                    AddWindow(command.Window);
                    break;
                }

                case Command.DISMISS_WINDOW:
                {
                    DismissWindow(command.Window.Id);
                    break;
                }

                case Command.SET_ACTIVE_STATE:
                {
                    if (command.Window.CurrentState == LifecycleState.ACTIVE)
                    {
                        break;
                    }
                    m_inactiveWindows.Remove(command.Window.Id);
                    SetWindowActive(command.Window);
                    break;
                }

                case Command.SET_INACTIVE_STATE:
                {
                    if (command.Window.CurrentState == LifecycleState.INACTIVE)
                    {
                        break;
                    }
                    m_activeWindows.Remove(command.Window.Id);
                    SetWindowInActive(command.Window);
                    break;
                }
            }
        }
        
        private void DismissWindow(int id)
        {
            if (!m_windows.TryGetValue(id, out var flowWindow))
            {
                return;
            }
            
            switch (flowWindow.CurrentState)
            {
                case LifecycleState.ACTIVE:
                {
                    m_activeWindows.Remove(id);
                    break;
                }

                case LifecycleState.INACTIVE:
                {
                    m_inactiveWindows.Remove(id);
                    break;
                }

                case LifecycleState.PRESENTING:
                {
                    m_presentingWindows.Remove(id);
                    break;
                }
            }

            m_dismissingWindows.Insert(id, flowWindow);
            
            flowWindow.CurrentState = LifecycleState.DISMISSING;
            flowWindow.OnDismissingStart();
        }

        private void RemoveWindow(int id)
        {
            var flowWindow = m_windows.GetValue(id);
            flowWindow.OnDismissed();
            
            m_windows.Remove(id);
            m_dismissingWindows.Remove(id);
        }

        private void AddWindow(FlowWindow flowWindow)
        {
            if (m_windows.Contains(state => state == flowWindow))
            {
                Debug.LogWarning("Trying to add duplicate entry of same FlowWindow in FlowState");
                return;
            }
            
            flowWindow.Owner = this;
            flowWindow.CurrentState = LifecycleState.PRESENTING;
            flowWindow.Id = m_windows.Insert(flowWindow);
            
            
            
            //m_activeWindows.Insert(flowWindow.Id, flowWindow);
            
            // add on init to windows
            m_presentingWindows.Insert(flowWindow.Id, flowWindow);
        }

        private void SetWindowActive(FlowWindow flowWindow)
        {
            Debug.Assert(m_windows.Contains(flowWindow.Id));
            
            flowWindow.OnActiveStart();
            flowWindow.CurrentState = LifecycleState.ACTIVE;

            m_activeWindows.Insert(flowWindow.Id, flowWindow);
        }

        private void SetWindowInActive(FlowWindow flowWindow)
        {
            Debug.Assert(m_windows.Contains(flowWindow.Id));
            
            flowWindow.OnInActiveStart();
            flowWindow.CurrentState = LifecycleState.INACTIVE;

            m_inactiveWindows.Insert(flowWindow.Id, flowWindow);
        }
        
        #endregion
    }
}