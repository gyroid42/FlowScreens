using System;
using System.Runtime.InteropServices;


namespace FlowStates.FlowMessageUnion
{
    public enum MenuNavigation : byte
    {
        MAIN_MENU = 0,
        SCREEN_1  = 1,
        SCREEN_2  = 2,
        BACK      = 3,
        QUIT      = 4
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