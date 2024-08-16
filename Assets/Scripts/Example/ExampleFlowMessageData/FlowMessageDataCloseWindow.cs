using System;
using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataCloseWindow
    {
        public WindowType window;
    }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataCloseWindow _CloseWindow;
        
        public ref FlowMessageDataCloseWindow CloseWindow
        {
            get
            {
                fixed (FlowMessageDataCloseWindow* p = &_CloseWindow)
                {
                    if (_field_used_ != FlowMessageType.CLOSE_WINDOW)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.CLOSE_WINDOW;
                    }

                    return ref *p;
                }
            }
        }
    }
}