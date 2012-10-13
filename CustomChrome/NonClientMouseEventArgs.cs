using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    public class NonClientMouseEventArgs : MouseEventArgs
    {
        public int HitTest { get; set; }

        public bool Handled { get; set; }

        public NonClientMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, int hitTest)
            : base(button, clicks, x, y, delta)
        {
            HitTest = hitTest;
        }
    }

    public delegate void NonClientMouseEventHandler(object sender, NonClientMouseEventArgs e);
}
