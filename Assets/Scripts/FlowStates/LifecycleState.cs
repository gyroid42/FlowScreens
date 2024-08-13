namespace FlowStates
{
    public enum LifecycleState : byte
    {
        DEFAULT,
        INITIALISING,
        ACTIVE,
        PRESENTING,
        DISMISSING,
        DISMISSED,
        INACTIVE
    }
}