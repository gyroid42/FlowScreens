using System;
using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataConfirmPopup
    {
        public bool confirm;
    }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataConfirmPopup _ConfirmPopup;
        
        public ref FlowMessageDataConfirmPopup ConfirmPopup
        {
            get
            {
                fixed (FlowMessageDataConfirmPopup* p = &_ConfirmPopup)
                {
                    if (_field_used_ != FlowMessageType.CONFIRM_POPUP)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.CONFIRM_POPUP;
                    }

                    return ref *p;
                }
            }
        }
    }
}