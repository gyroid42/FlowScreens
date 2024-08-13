using System.Runtime.InteropServices;

namespace FlowStates.FlowMessageUnion
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(0)]
        private FlowMessageType _field_used_;
        public FlowMessageType Field => _field_used_;
    }
}