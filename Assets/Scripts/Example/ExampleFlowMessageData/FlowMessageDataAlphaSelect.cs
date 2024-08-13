using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FlowStates.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataAlphaSelect
    {
        [Range(0f, 1f)] public float alpha;
    }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataAlphaSelect _AlphaSelect;
        
        public ref FlowMessageDataAlphaSelect AlphaSelect
        {
            get
            {
                fixed (FlowMessageDataAlphaSelect* p = &_AlphaSelect)
                {
                    if (_field_used_ != FlowMessageType.ALPHA_SELECT)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.ALPHA_SELECT;
                    }

                    return ref *p;
                }
            }
        }
    }
}