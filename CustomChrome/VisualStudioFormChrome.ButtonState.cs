using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    partial class VisualStudioFormChrome
    {
        private class ButtonStates
        {
            public ChromeButton OverButton { get; private set; }
            public ChromeButton DownButton { get; private set; }
            public bool DrawIcon { get; private set; }
            public ButtonState Minimize { get; private set; }
            public ButtonState MaximizeRestore { get; private set; }
            public ButtonState Close { get; private set; }
            public int RightOffset { get; private set; }

            public ButtonStates(VisualStudioFormChrome parent)
            {
                bool drawMinimize = false;
                bool drawMaximizeRestore = false;
                bool drawClose = false;
                bool enableMinimize = false;
                bool enableMaximizeRestore = false;
                bool enableClose = false;

                if (parent.Form.ControlBox)
                {
                    DrawIcon = parent.GetFormIcon() != null && parent.Form.ShowIcon;
                    drawClose = true;
                    drawMinimize = drawMaximizeRestore = parent.Form.MinimizeBox || parent.Form.MaximizeBox;
                    enableMinimize = parent.Form.MinimizeBox;
                    enableMaximizeRestore = parent.Form.MaximizeBox;
                    enableClose = CanClose(parent);
                }

                var rightOffset =
                    parent.Form.Width -
                    parent._formChrome.AdjustedResizeBorderThickness.Left;

                if (drawClose)
                    Close = GetButtonState(parent, ref rightOffset, ChromeButton.Close, enableClose);
                else
                    Close = new ButtonState(ChromeButton.Close);
                if (drawMaximizeRestore)
                    MaximizeRestore = GetButtonState(parent, ref rightOffset, ChromeButton.MaximizeRestore, enableMaximizeRestore);
                if (drawMinimize)
                    Minimize = GetButtonState(parent, ref rightOffset, ChromeButton.Minimize, enableMinimize);

                RightOffset = rightOffset;
            }

            private ButtonState GetButtonState(VisualStudioFormChrome parent, ref int offset, ChromeButton button, bool enabled)
            {
                offset -= ButtonSize.Width;

                var bounds = new Rectangle(
                    offset,
                    parent._formChrome.AdjustedResizeBorderThickness.Top,
                    ButtonSize.Width,
                    ButtonSize.Height
                );

                var location = parent._formChrome.PointToClient(Cursor.Position);
                bool over = bounds.Contains(location);

                ChromeButtonState state;

                if (!enabled)
                {
                    state = ChromeButtonState.Disabled;
                }
                else if (parent.CaptureStart.HasValue)
                {
                    if (bounds.Contains(parent.CaptureStart.Value))
                        state = over ? ChromeButtonState.Down : ChromeButtonState.Over;
                    else
                        state = ChromeButtonState.Enabled;
                }
                else
                {
                    state = over ? ChromeButtonState.Over : ChromeButtonState.Enabled;
                }

                if (state == ChromeButtonState.Over)
                    OverButton = button;
                else if (state == ChromeButtonState.Down)
                    DownButton = button;

                return new ButtonState(button, state, true, bounds);
            }

            private bool CanClose(VisualStudioFormChrome parent)
            {
                if (!parent.Form.IsHandleCreated)
                    return false;

                uint menuState = NativeMethods.GetMenuState(
                    NativeMethods.GetSystemMenu(parent.Form.Handle, false),
                    NativeMethods.SC_CLOSE,
                    NativeMethods.MF_BYCOMMAND
                );

                return (menuState & (NativeMethods.MF_GRAYED | NativeMethods.MF_DISABLED)) == 0;
            }
        }

        private class ButtonState
        {
            public ChromeButton Button { get; private set; }
            public ChromeButtonState State { get; private set; }
            public bool Draw { get; private set; }
            public Rectangle Bounds { get; private set; }

            public ButtonState(ChromeButton button)
                : this(button, ChromeButtonState.Disabled, false, Rectangle.Empty)
            {
            }

            public ButtonState(ChromeButton button, ChromeButtonState state, bool draw, Rectangle bounds)
            {
                Button = button;
                State = state;
                Draw = draw;
                Bounds = bounds;
            }
        }
    }
}
