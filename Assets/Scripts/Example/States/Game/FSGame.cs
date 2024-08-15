using FlowStates;

namespace Example
{
    public class FSGame : FlowState
    {
        public FSGame(FlowStateContext context) : base(context)
        {
            Context.AnimationSpeed = 1f;
        }

        internal override void OnInit()
        {
            OwningFSM.PushState(new FSMainMenu(Context));
        }
    }
}