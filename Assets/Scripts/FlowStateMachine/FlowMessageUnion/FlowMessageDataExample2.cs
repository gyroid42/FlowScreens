using System;
using System.Runtime.InteropServices;

namespace FlowStateMachine.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataExample2
    {
        public int value1;
        public float value2;
    }
    
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataExample2 _Example2;
        
        public ref FlowMessageDataExample2 Example2
        {
            get
            {
                fixed (FlowMessageDataExample2* p = &_Example2)
                {
                    if (_field_used_ != FlowMessageType.EXAMPLE_2)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.EXAMPLE_2;
                    }

                    return ref *p;
                }
            }
        }
    }
}