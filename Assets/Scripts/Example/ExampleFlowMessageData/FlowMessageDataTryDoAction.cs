using System;
using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataTryDoAction { }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataTryDoAction _TryDoAction;

        public ref FlowMessageDataTryDoAction TryDoAction
        {
            get
            {
                fixed (FlowMessageDataTryDoAction* p = &_TryDoAction)
                {
                    if (_field_used_ != FlowMessageType.TRY_DO_ACTION)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.TRY_DO_ACTION;
                    }

                    return ref *p;
                }
            }
        }
    }
}