using System;
using System.Collections.Generic;
using System.Text;

namespace CustomChrome
{
    public enum SystemCommand
    {
        Size = 0xF000,
        Move = 0xF010,
        Minimize = 0xF020,
        Maximize = 0xF030,
        DoubleClickMaximize = 0xF032,
        NextWindow = 0xF040,
        PrevWindow = 0xF050,
        Close = 0xF060,
        VScroll = 0xF070,
        HScroll = 0xF080,
        MouseMenu = 0xF090,
        Keymenu = 0xF100,
        Arrange = 0xF110,
        Restore = 0xF120,
        DoubleClickRestore = 0xF122,
        Tasklist = 0xF130,
        Screensave = 0xF140,
        Hotkey = 0xF150,
        Default = 0xF160,
        Monitorpower = 0xF170,
        Contexthelp = 0xF180,
        Separator = 0xF00F
    }
}
