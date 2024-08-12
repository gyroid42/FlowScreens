using System;
using System.Runtime.InteropServices;

namespace FlowStateMachine.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataExample1
    {
        public long value1;
    }
    
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataExample1 _Example1;
        
        public ref FlowMessageDataExample1 Example1
        {
            get
            {
                fixed (FlowMessageDataExample1* p = &_Example1)
                {
                    if (_field_used_ != FlowMessageType.EXAMPLE_1)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.EXAMPLE_1;
                    }

                    return ref *p;
                }
            }
        }
    }
}