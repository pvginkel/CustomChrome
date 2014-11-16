using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CustomChrome
{
    public class VisualStudioButtonPaintEventArgs : NonClientPaintEventArgs
    {
        public VisualStudioButton Button { get; private set; }

        public Color ForeColor { get; private set; }

        public Color BackColor { get; private set; }

        public VisualStudioButtonPaintEventArgs(VisualStudioButton button, Graphics graphics, Rectangle bounds, Region clipRegion, bool isMaximized, bool isActive)
            : base(graphics, bounds, clipRegion, isMaximized, isActive)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            Button = button;

            ChromeButtonState state = 0;
            if (button.IsOver)
                state = ChromeButtonState.Over;
            else if (button.IsDown)
                state = ChromeButtonState.Down;

            ForeColor = button.Chrome.GetForeColor(button.Enabled, state);
            BackColor = button.Chrome.GetBackColor(button.Enabled, state);
        }

        public void PaintBackground()
        {
            ChromeButtonState state = 0;
            if (Button.IsOver)
                state = ChromeButtonState.Over;
            else if (Button.IsDown)
                state = ChromeButtonState.Down;

            Graphics.FillRectangle(Button.Chrome.GetButtonBackgroundBrush(state), Bounds);
        }
    }

    public delegate void VisualStudioButtonPaintEventHandler(object sender, VisualStudioButtonPaintEventArgs e);
}
