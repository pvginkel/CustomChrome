using System;
using System.Collections.Generic;
using System.Text;

namespace CustomChrome
{
    internal enum NonClientHitTest
    {
        Error = -2,
        Transparent = -1,
        Nowhere = 0,
        Client = 1,
        Caption = 2,
        SystemMenu = 3,
        GrowBox = 4,
        Menu = 5,
        HorizontalScroll = 6,
        VerticalScroll = 7,
        MinButton = 8,
        MaxButton = 9,
        Left = 10,
        Right = 11,
        Top = 12,
        TopLeft = 13,
        TopRight = 14,
        Bottom = 15,
        BottomLeft = 16,
        BottomRight = 17,
        Border = 18,
        Object = 19,
        Close = 20,
        Help = 21
    }
}
