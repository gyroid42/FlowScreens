using FlowStates.FlowMessageUnion;

namespace FlowStates
{
    public abstract class FlowWindow
    {
        public LifecycleState CurrentState { get; internal set; }
        public FlowStateMachine OwningFSM { get; internal set; }
        public FlowState Owner { get; internal set; }
        public FlowStateContext Context { get; internal set; }
        public byte Id { get; internal set; }

        protected FlowWindow(FlowStateContext context)
        {
            Context = context;
        }
        
        public virtual void OnInit() { }
        public virtual FlowProgress OnInitUpdate() => FlowProgress.COMPLETE;
        public virtual void LinkFlowGroups() { }
        public virtual void OnFlowMessageReceived(in FlowMessageData message) { }
        
        public virtual void OnPresentingStart() { }
        public virtual FlowProgress OnPresentingUpdate()
        {
            return FlowProgress.COMPLETE;
        }
        
        public virtual void OnActiveStart() { }
        public virtual void OnActiveUpdate() { }
        public virtual void OnActiveLateUpdate() { }
        public virtual void OnActiveFixedUpdate() { }
        
        public virtual void OnInActiveStart() { }
        public virtual void OnInActiveUpdate() { }
        
        public virtual void OnDismissingStart() { }
        public virtual FlowProgress OnDismissingUpdate()
        {
            return FlowProgress.COMPLETE;
        }
        public virtual void OnDismissed() { }
    }
}