using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Collections;
using FlowStates.FlowMessageUnion;
using UnityEngine;

namespace FlowStates
{
    public abstract class FlowState
    {
        private const int k_windowCapacity = 20;
        private const byte k_commandCapacity = 32;
        
        private enum Command : byte
        {
            ADD_WINDOW,
            DISMISS_WINDOW,
            SET_WINDOW_ACTIVE,
            SET_WINDOW_INACTIVE
        }

        private struct FlowWindowCommand
        {
            public Command Command;
            public byte WindowId;
        }
        
        private SparseArray<FlowWindow> m_windows = new SparseArray<FlowWindow>(k_windowCapacity);
        private SparseArray<FlowWindow> m_activeWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private SparseArray<FlowWindow> m_inactiveWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private SparseArray<FlowWindow> m_dismissingWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private SparseArray<FlowWindow> m_presentingWindows = new SparseArray<FlowWindow>(k_windowCapacity);
        private SparseArray<FlowWindow> m_initialisingWindows = new SparseArray<FlowWindow>(k_windowCapacity);

        private FixedQueue<FlowWindowCommand> m_commandQueue = new FixedQueue<FlowWindowCommand>(k_commandCapacity);
        private SparseArray<byte> m_pendingWindowsToDismiss = new SparseArray<byte>(k_windowCapacity);
        
        public FlowStateMachine OwningFSM { get; internal set; }
        private Task m_initTask;
        public int Id { get; internal set; }
        internal LifecycleState CurrentLifecycleState;
        protected FlowStateContext Context;

        protected FlowState(in FlowStateContext context)
        {
            Context = context;
        }
        
        #region Public API

        protected byte AddWindow(FlowWindow window)
        {
            bool found = false;
            for (int i = 0; i < m_windows.Length; i++)
            {
                if (m_windows[i] != window)
                {
                    continue;
                }
                
                found = true;
                break;
            }

            if (found)
            {
                Debug.LogWarning("Trying to add duplicate entry of same FlowWindow in FlowState");
                return byte.MaxValue;
            }
            
            window.Id = (byte) m_windows.Insert(window);
            
            m_commandQueue.Enqueue(new FlowWindowCommand
            {
                Command = Command.ADD_WINDOW,
                WindowId = window.Id
            });

            return window.Id;
        }

        protected void DismissWindow(byte windowId)
        {
            m_commandQueue.Enqueue(new FlowWindowCommand
            {
                Command = Command.DISMISS_WINDOW,
                WindowId = windowId
            });
        }

        public void SetWindowActive(byte windowId)
        {
            m_commandQueue.Enqueue(new FlowWindowCommand
            {
                Command = Command.SET_WINDOW_ACTIVE,
                WindowId = windowId
            });
        }

        public void SetWindowInactive(byte windowId)
        {
            m_commandQueue.Enqueue(new FlowWindowCommand
            {
                Command = Command.SET_WINDOW_INACTIVE,
                WindowId = windowId
            });
        }
        
        #endregion
        
        #region Internal API
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SendMessageToWindow(in FlowMessageData message, byte windowId)
        {
            if (m_windows.TryGetValue(windowId, out var window) &&
                (window.CurrentState == LifecycleState.ACTIVE || window.CurrentState == LifecycleState.PRESENTING))
            {
                window.OnFlowMessageReceived(message);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnActiveStartInternal()
        {
            for (var i = 0; i < m_activeWindows.Length; i++)
            {
                var window = m_activeWindows[i];
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

            for (int i = 0; i < m_initialisingWindows.Length; i++)
            {
                var window = m_initialisingWindows[i];
                FlowProgress progress = window.OnInitUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    window.LinkFlowGroups(Id);
                    m_initialisingWindows.Remove(window.Id);
                    SetWindowPresenting(window);
                }
            }

            for (var i = 0; i < m_presentingWindows.Length; i++)
            {
                var window = m_presentingWindows[i];
                FlowProgress progress = window.OnPresentingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    m_presentingWindows.Remove(window.Id);
                    SetWindowActive(window);

                    if (m_pendingWindowsToDismiss.Contains(window.Id))
                    {
                        DismissWindow(window);
                        m_pendingWindowsToDismiss.Remove(window.Id);
                    }
                }
            }
            
            for (var i = 0; i < m_activeWindows.Length; i++)
            {
                var window = m_activeWindows[i];
                window.OnActiveUpdate();
            }
            
            for (var i = 0; i < m_inactiveWindows.Length; i++)
            {
                var window = m_inactiveWindows[i];
                window.OnInActiveUpdate();
            }
            
            for (var i = 0; i < m_dismissingWindows.Length; i++)
            {
                var window = m_dismissingWindows[i];
                FlowProgress progress = window.OnDismissingUpdate();
                if (progress == FlowProgress.COMPLETE)
                {
                    RemoveWindow(window.Id);
                }
            }
            
            OnActiveUpdate();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnActiveLateUpdateInternal()
        {
            for (var i = 0; i < m_activeWindows.Length; i++)
            {
                var window = m_activeWindows[i];
                window.OnActiveLateUpdate();
            }
            
            OnActiveLateUpdate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnActiveFixedUpdateInternal()
        {
            for (var i = 0; i < m_activeWindows.Length; i++)
            {
                var window = m_activeWindows[i];
                window.OnActiveFixedUpdate();
            }
            
            OnActiveFixedUpdate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnDismissingStartInternal()
        {
            for (var i = 0; i < m_windows.Length; i++)
            {
                var window = m_windows[i];

                if (window.CurrentState != LifecycleState.DISMISSING &&
                    window.CurrentState != LifecycleState.DISMISSED)
                {
                    DismissWindow(window);
                }
            }
            
            OnDismissingStart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FlowProgress OnDismissingUpdateInternal()
        {
            for (int i = 0; i < m_presentingWindows.Length; i++)
            {
                var window = m_presentingWindows[i];
                if (window.OnPresentingUpdate() == FlowProgress.COMPLETE)
                {
                    m_presentingWindows.Remove(window.Id);
                    SetWindowActive(window);

                    DismissWindow(window);
                }
            }
            
            for (var i = 0; i < m_dismissingWindows.Length; i++)
            {
                var window = m_dismissingWindows[i];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnInitInternal()
        {
            m_initTask = OnInit();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FlowProgress OnInitUpdate()
        {
            if (m_initTask is { IsCompleted: false })
            {
                return FlowProgress.PROGRESSING;
            }

            return FlowProgress.COMPLETE;
        }

        
        #endregion
        
        #region Virtual Methods

        internal virtual void OnFlowMessageReceived(in FlowMessageData message) { }

        protected virtual Task OnInit() => null;
        internal virtual void LinkFlowGroups() { }
        
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
            
            if (!m_windows.TryGetValue(command.WindowId, out var window))
            {
                return;
            }
            
            switch (command.Command)
            {
                case Command.ADD_WINDOW:
                {
                    HandleAddWindowCommand(window);
                    break;
                }

                case Command.DISMISS_WINDOW:
                {
                    DismissWindow(window);
                    break;
                }

                case Command.SET_WINDOW_ACTIVE:
                {
                    if (window.CurrentState != LifecycleState.INACTIVE)
                    {
                        break;
                    }
                    m_inactiveWindows.Remove(command.WindowId);
                    SetWindowActive(window);
                    break;
                }

                case Command.SET_WINDOW_INACTIVE:
                {
                    if (window.CurrentState != LifecycleState.ACTIVE)
                    {
                        break;
                    }
                    m_activeWindows.Remove(command.WindowId);
                    SetWindowInActive(window);
                    break;
                }
            }
        }
        
        private void DismissWindow(FlowWindow flowWindow)
        {
            switch (flowWindow.CurrentState)
            {
                case LifecycleState.ACTIVE:
                {
                    m_activeWindows.Remove(flowWindow.Id);
                    break;
                }

                case LifecycleState.INACTIVE:
                {
                    m_inactiveWindows.Remove(flowWindow.Id);
                    break;
                }

                case LifecycleState.PRESENTING:
                {
                    m_pendingWindowsToDismiss.Insert(flowWindow.Id, flowWindow.Id);
                    return;
                }
            }

            m_dismissingWindows.Insert(flowWindow.Id, flowWindow);
            
            flowWindow.CurrentState = LifecycleState.DISMISSING;
            flowWindow.OnDismissingStart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveWindow(int id)
        {
            var flowWindow = m_windows.GetValue(id);
            flowWindow.OnDismissed();
            
            m_windows.Remove(id);
            m_dismissingWindows.Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleAddWindowCommand(FlowWindow flowWindow)
        {
            flowWindow.Owner = this;
            flowWindow.OwningFSM = OwningFSM;
            flowWindow.CurrentState = LifecycleState.INITIALISING;
            
            m_initialisingWindows.Insert(flowWindow.Id, flowWindow);
            flowWindow.OnInit();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetWindowActive(FlowWindow flowWindow)
        {
            flowWindow.CurrentState = LifecycleState.ACTIVE;
            m_activeWindows.Insert(flowWindow.Id, flowWindow);
            
            flowWindow.OnActiveStart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetWindowInActive(FlowWindow flowWindow)
        {
            flowWindow.CurrentState = LifecycleState.INACTIVE;
            m_inactiveWindows.Insert(flowWindow.Id, flowWindow);
            
            flowWindow.OnInActiveStart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetWindowPresenting(FlowWindow flowWindow)
        {
            Debug.Assert(m_windows.Contains(flowWindow.Id));

            flowWindow.CurrentState = LifecycleState.PRESENTING;
            m_presentingWindows.Insert(flowWindow.Id, flowWindow);

            flowWindow.OnPresentingStart();
        }
        
        #endregion
    }
}