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
            public VisualStudioButton OverExtraButton { get; private set; }
            public ChromeButton DownButton { get; private set; }
            public VisualStudioButton DownExtraButton { get; private set; }
            public bool DrawIcon { get; private set; }
            public ButtonState Minimize { get; private set; }
            public ButtonState MaximizeRestore { get; private set; }
            public ButtonState Close { get; private set; }
            public List<ButtonState> ExtraButtons { get; private set; }
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
                    Close = GetButtonState(parent, ref rightOffset, ChromeButton.Close, enableClose, null);
                else
                    Close = new ButtonState(ChromeButton.Close);
                if (drawMaximizeRestore)
                    MaximizeRestore = GetButtonState(parent, ref rightOffset, ChromeButton.MaximizeRestore, enableMaximizeRestore, null);
                if (drawMinimize)
                    Minimize = GetButtonState(parent, ref rightOffset, ChromeButton.Minimize, enableMinimize, null);

                ProcessExtraButtons(parent, ref rightOffset);

                RightOffset = rightOffset;
            }

            private void ProcessExtraButtons(VisualStudioFormChrome parent, ref int rightOffset)
            {
                ExtraButtons = new List<ButtonState>(parent.Buttons.Count);

                for (int i = parent.Buttons.Count - 1; i >= 0; i--)
                {
                    var extraButton = parent.Buttons[i];
                    if (!extraButton.Visible)
                        continue;

                    ExtraButtons.Insert(
                        0,
                        GetButtonState(
                            parent,
                            ref rightOffset,
                            ChromeButton.None,
                            extraButton.Enabled,
                            extraButton
                        )
                    );
                }
            }

            private ButtonState GetButtonState(VisualStudioFormChrome parent, ref int offset, ChromeButton button, bool enabled, VisualStudioButton extraButton)
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

                if (extraButton != null)
                {
                    extraButton.IsOver = false;
                    extraButton.IsDown = false;
                }

                if (state == ChromeButtonState.Over)
                {
                    OverButton = button;
                    OverExtraButton = extraButton;
                    if (extraButton != null)
                        extraButton.IsOver = true;
                }
                else if (state == ChromeButtonState.Down)
                {
                    DownButton = button;
                    DownExtraButton = extraButton;
                    if (extraButton != null)
                        extraButton.IsDown = true;
                }

                return new ButtonState(button, state, true, bounds, extraButton);
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
            public VisualStudioButton ExtraButton { get; private set; }
            public ChromeButtonState State { get; private set; }
            public bool Draw { get; private set; }
            public Rectangle Bounds { get; private set; }

            public ButtonState(ChromeButton button)
                : this(button, ChromeButtonState.Disabled, false, Rectangle.Empty, null)
            {
            }

            public ButtonState(ChromeButton button, ChromeButtonState state, bool draw, Rectangle bounds, VisualStudioButton extraButton)
            {
                Button = button;
                State = state;
                Draw = draw;
                Bounds = bounds;
                ExtraButton = extraButton;
            }
        }
    }
}
