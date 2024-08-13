using System;
using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    [Serializable]
    public struct FlowMessageDataSetAnimationSpeed
    {
        public float speed;
    }

    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataSetAnimationSpeed _SetAnimationSpeed;

        public ref FlowMessageDataSetAnimationSpeed SetAnimationSpeed
        {
            get
            {
                fixed (FlowMessageDataSetAnimationSpeed* p = &_SetAnimationSpeed)
                {
                    if (_field_used_ != FlowMessageType.SET_ANIMATION_SPEED)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.SET_ANIMATION_SPEED;
                    }

                    return ref *p;
                }
            }
        }
    }
}