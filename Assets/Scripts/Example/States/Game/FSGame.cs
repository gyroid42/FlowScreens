using System.Threading.Tasks;
using FlowStates;

namespace Example
{
    public class FSGame : FlowState
    {
        public FSGame(FlowStateContext context) : base(context)
        {
            Context.AnimationSpeed = 1f;
        }

        protected override Task OnInit()
        {
            OwningFSM.PushState(new FSMainMenu(Context));
            return null;
        }
    }
}