﻿namespace FlowState
{
    public abstract class FlowWindow
    {
        public LifecycleState CurrentState { get; internal set; }
        public FlowState Owner { get; internal set; }
        public int Id { get; internal set; }
        
        public virtual void OnInit() { }
        public virtual FlowProgress OnInitUpdate() => FlowProgress.COMPLETE;
        
        public virtual FlowProgress OnPresentingUpdate()
        {
            return FlowProgress.COMPLETE;
        }
        
        public virtual void OnActiveStart() { }
        public virtual void OnActiveUpdate() { }
        public virtual void OnInActiveUpdate() { }

        public virtual FlowProgress OnDismissingUpdate()
        {
            return FlowProgress.COMPLETE;
        }
        public virtual void OnActiveLateUpdate() { }
        public virtual void OnActiveFixedUpdate() { }
        public virtual void OnDismissingStart() { }
        public virtual void OnDismissed() { }
        public virtual void OnInActiveStart() { }
        public virtual void OnFlowMessageReceived(object message) { }
    }
}