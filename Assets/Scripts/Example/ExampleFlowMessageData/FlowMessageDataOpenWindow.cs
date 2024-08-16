using System;
using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    public enum OpenWindow : byte
    {
        EXAMPLE_1    = 0
    }
    
    [Serializable]
    public struct FlowMessageDataOpenWindow
    {
        public OpenWindow window;
    }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataOpenWindow _OpenWindow;
        
        public ref FlowMessageDataOpenWindow OpenWindow
        {
            get
            {
                fixed (FlowMessageDataOpenWindow* p = &_OpenWindow)
                {
                    if (_field_used_ != FlowMessageType.OPEN_WINDOW)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.OPEN_WINDOW;
                    }

                    return ref *p;
                }
            }
        }
    }
}