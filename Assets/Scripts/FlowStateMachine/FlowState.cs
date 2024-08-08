using System.Collections.Generic;
using Collections;
using UnityEngine;

namespace FlowState
{
    public abstract class FlowState
    {
        private const int k_windowCapacity = 20;
        
        private enum Command : byte
        {
            ADD_WINDOW,
            DISMISS_WINDOW,
            SET_WINDOW_ACTIVE,
            SET_WINDOW_INACTIVE
        }
        
        public FlowStateMachine OwningFSM { get; internal set; }
        public LifecycleState CurrentLifecycleState { get; internal set; }
        public byte Id { get; internal set; }
        
        #region Virtual Methods

        internal virtual void OnFlowMessageReceived(FlowMessageData message) { }

        internal virtual void OnInit() {  }
        internal virtual FlowProgress OnInitUpdate() => FlowProgress.COMPLETE;
        
        
        internal virtual void OnPresentingStart() { }
        internal virtual FlowProgress OnPresentingUpdate() => FlowProgress.COMPLETE;
        
        
        internal virtual void OnActiveStart() { }
        internal virtual void OnActiveUpdate() { }
        internal virtual void OnActiveLateUpdate() { }
        internal virtual void OnActiveFixedUpdate() { }
        
        
        internal virtual void OnDismissingStart() { }
        internal virtual FlowProgress OnDismissingUpdate() => FlowProgress.COMPLETE;
        internal virtual void OnDismissed() { }
        
        
        internal virtual void OnInActiveStart() { }
        internal virtual void OnInActiveUpdate() { }

        #endregion
    }
}