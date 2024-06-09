namespace FlowState
{
    public abstract class FlowState
    {
        public readonly FlowScreen Owner;
        public int Key;

        protected FlowState(FlowScreen owner)
        {
            Owner = owner;
        }
        
        public abstract void OnActiveStart();
        public abstract FlowProgress OnPresentingUpdate();
        public abstract void OnActiveUpdate();
        public abstract void OnInActiveUpdate();
        public abstract FlowProgress OnDismissingUpdate();
        public abstract void OnActiveLateUpdate();
        public abstract void OnActiveFixedUpdate();
        public abstract void OnDismissingStart();
        public abstract void OnDismissed();
        public abstract void OnInActiveStart();
        
    }
}