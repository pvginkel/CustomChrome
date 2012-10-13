using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CustomChrome
{
    public class NonClientPaintEventArgs : EventArgs
    {
        public Rectangle Bounds { get; private set; }
        public Region ClipRegion { get; private set; }
        public bool IsMaximized { get; set; }
        public bool IsActive { get; set; }
        public Graphics Graphics { get; private set; }

        public NonClientPaintEventArgs(Graphics graphics, Rectangle bounds, Region clipRegion, bool isMaximized, bool isActive)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Graphics = graphics;
            Bounds = bounds;
            ClipRegion = clipRegion;
            IsMaximized = isMaximized;
            IsActive = isActive;
        }
    }

    public delegate void NonClientPaintEventHandler(object sender, NonClientPaintEventArgs e);
}
