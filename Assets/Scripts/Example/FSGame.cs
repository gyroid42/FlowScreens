using FlowStates;
using UnityEngine;

namespace Example
{
    public class FSGame : FlowState
    {
        public FSGame(FlowStateContext context) : base(context) { }

        internal override void OnInit()
        {
            OwningFSM.PushState(new FSMainMenu(Context));
        }
    }
}