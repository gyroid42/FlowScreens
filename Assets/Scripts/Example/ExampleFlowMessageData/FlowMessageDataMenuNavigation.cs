using System;
using System.Runtime.InteropServices;
using FlowStates.FlowMessageUnion;


namespace FlowStates.FlowMessageUnion
{
    public enum MenuNavigation : byte
    {
        MAIN_MENU,
        SCREEN_1,
        SCREEN_2,
        BACK
    }
    
    [Serializable]
    public struct FlowMessageDataMenuNavigation
    {
        public MenuNavigation navigation;
    }
    
    public unsafe partial struct FlowMessageData
    {
        [FieldOffset(8)] private FlowMessageDataMenuNavigation _MenuNavigation;
        
        public ref FlowMessageDataMenuNavigation MenuNavigation
        {
            get
            {
                fixed (FlowMessageDataMenuNavigation* p = &_MenuNavigation)
                {
                    if (_field_used_ != FlowMessageType.MENU_NAVIGATION)
                    {
                        *p = default;
                        _field_used_ = FlowMessageType.MENU_NAVIGATION;
                    }

                    return ref *p;
                }
            }
        }
    }
}