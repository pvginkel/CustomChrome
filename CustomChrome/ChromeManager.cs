using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal class ChromeManager : NativeWindow
    {
        private readonly FormChrome _chrome;
        private readonly Form _form;
        private NativeMethods.WINDOWPOS? _runningWindowPos;

        public bool IsMaximized
        {
            get { return (NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE) & NativeMethods.WS_MAXIMIZE) != 0; }
        }

        public bool IsActive
        {
            get { return NativeMethods.GetForegroundWindow() == Handle; }
        }

        public ChromeManager(Form form, FormChrome chrome)
        {
            if (form == null)
                throw new ArgumentNullException("form");
            if (chrome == null)
                throw new ArgumentNullException("chrome");

            _form = form;
            _chrome = chrome;
        }

        public void Initialize()
        {
            if (_form.IsHandleCreated)
            {
                AssignHandle(_form.Handle);
                InitializeForm();
            }
            else
            {
                _form.HandleCreated += form_HandleCreated;
            }

            _form.HandleDestroyed += form_HandleDestroyed;
        }

        void form_HandleCreated(object sender, EventArgs e)
        {
            AssignHandle(_form.Handle);

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Disable theming on current window so we don't get 
            // any funny artifacts (round corners, etc.)
            NativeMethods.SetWindowTheme(Handle, "", "");

#if !DEBUG
			// When application window stops responding to messages
			// system will finally loose patience and repaint it with default theme.
			// This prevents such behavior for entire application.
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/lowlevelclientsupport/misc/rtldisableprocesswindowsghosting.asp

			// NativeMethods.DisableProcessWindowsGhosting();
#endif
        }

        public override void ReleaseHandle()
        {
            RestoreForm();

            base.ReleaseHandle();
        }

        private void RestoreForm()
        {
            NativeMethods.SetWindowTheme(Handle, null, null);

            // The title bar won't redraw on it's own. This fix works around this.

            string text = _form.Text;
            _form.Text = null;

            _form.BeginInvoke(new Action(() =>
            {
                _form.Text = text;
            }));
        }

        void form_HandleDestroyed(object sender, EventArgs e)
        {
            ReleaseHandle();
        }

        public override void CreateHandle(CreateParams cp)
        {
            cp.Style = cp.Style | NativeMethods.WS_SYSMENU;

            base.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_NCCALCSIZE:
                    WmNCCalcSize(ref m);
                    break;

                case NativeMethods.WM_NCHITTEST:
                    WmNCHitTest(ref m);
                    break;

                case NativeMethods.WM_NCPAINT:
                    WmNCPaint(ref m);
                    break;

                case NativeMethods.WM_NCACTIVATE:
                    WmNCActivate(ref m);
                    break;

                case NativeMethods.WM_SETTEXT:
                    WmSetText(ref m);
                    break;

                case NativeMethods.WM_NCMOUSEMOVE:
                    WmNCMouseMove(ref m);
                    break;

                case NativeMethods.WM_NCMOUSELEAVE:
                    WmNCMouseLeave(ref m);
                    break;

                case NativeMethods.WM_NCLBUTTONDOWN:
                    WmNCLButtonDown(ref m);
                    break;

                case NativeMethods.WM_NCLBUTTONUP:
                    WmNCLButtonUp(ref m);
                    break;

                case NativeMethods.WM_NCUAHDRAWCAPTION:
                    break;

                case NativeMethods.WM_SYSCOMMAND:
                    WmSysCommand(ref m);
                    break;

                case NativeMethods.WM_WINDOWPOSCHANGING:
                    WmWindowPosChanging(ref m);
                    break;

                case NativeMethods.WM_WINDOWPOSCHANGED:
                    WmWindowPosChanged(ref m);
                    break;

                case NativeMethods.WM_ERASEBKGND:
                    WmEraseBkgnd(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void WmEraseBkgnd(ref Message m)
        {
            base.WndProc(ref m);

            _chrome.UpdateWindowState();
        }

        private void WmWindowPosChanged(ref Message m)
        {
            var previousWindowPos = _runningWindowPos;

            try
            {
                _runningWindowPos = (NativeMethods.WINDOWPOS)m.GetLParam(typeof(NativeMethods.WINDOWPOS));

                base.WndProc(ref m);

                _chrome.UpdateWindowState();
            }
            finally
            {
                _runningWindowPos = previousWindowPos;
            }
        }

        private void WmWindowPosChanging(ref Message m)
        {
            // During restore, there is a SetWindowPos which alters the
            // dimensions of the frame incorrectly. What happens is that the
            // non client size of the default frame gets added to the frame.
            // This detects this condition and suppresses the new size.

            var windowPos = (NativeMethods.WINDOWPOS)m.GetLParam(typeof(NativeMethods.WINDOWPOS));

            if (
                _runningWindowPos.HasValue && (
                    _runningWindowPos.Value.cx != windowPos.cx ||
                        _runningWindowPos.Value.cy != windowPos.cy
                    ) &&
                    (windowPos.flags & NativeMethods.SWP_FRAMECHANGED) == 0
                )
            {
                windowPos.cx = _runningWindowPos.Value.cx;
                windowPos.cy = _runningWindowPos.Value.cy;

                Marshal.StructureToPtr(windowPos, m.LParam, false);
            }

            base.WndProc(ref m);
        }

        private void WmSysCommand(ref Message m)
        {
            _chrome.OnSystemCommand(new SystemCommandEventArgs(
                (SystemCommand)m.WParam.ToInt32()
            ));

            base.WndProc(ref m);
        }

        public Point PointToClient(Point screenPoint)
        {
            return new Point(
                screenPoint.X - _form.Location.X,
                screenPoint.Y - _form.Location.Y
            );
        }

        private void WmNCCalcSize(ref Message m)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowmessages/wm_nccalcsize.asp
            // http://groups.google.pl/groups?selm=OnRNaGfDEHA.1600%40tk2msftngp13.phx.gbl

            if (m.WParam == IntPtr.Zero)
            {
                var ncRect = (NativeMethods.RECT)m.GetLParam(typeof(NativeMethods.RECT));
                var proposed = ncRect.ToRectangle();

                CalculateNonClientAreaSize(ref proposed);

                ncRect = new NativeMethods.RECT(proposed);

                Marshal.StructureToPtr(ncRect, m.LParam, false);
            }
            else if (m.WParam == (IntPtr)1)
            {
                var ncParams = (NativeMethods.NCCALCSIZE_PARAMS)m.GetLParam(typeof(NativeMethods.NCCALCSIZE_PARAMS));
                var proposed = ncParams.rectProposed.ToRectangle();

                CalculateNonClientAreaSize(ref proposed);

                ncParams.rectProposed = new NativeMethods.RECT(proposed);

                Marshal.StructureToPtr(ncParams, m.LParam, false);
            }

            m.Result = IntPtr.Zero;
        }

        private void WmNCHitTest(ref Message m)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/mouseinput/mouseinputreference/mouseinputmessages/wm_nchittest.asp

            var screenPoint = new Point(m.LParam.ToInt32());

            // convert to local coordinates
            var location = PointToClient(screenPoint);

            m.Result = (IntPtr)CalculateHitTest(location);
        }

        private void WmNCMouseMove(ref Message msg)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/mouseinput/mouseinputreference/mouseinputmessages/wm_nchittest.asp

            var clientPoint = PointToClient(new Point(msg.LParam.ToInt32()));

            _chrome.OnNonClientMouseMove(new MouseEventArgs(
                MouseButtons.None,
                0,
                clientPoint.X,
                clientPoint.Y,
                0
            ));

            msg.Result = IntPtr.Zero;
        }

        private void WmNCMouseLeave(ref Message m)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/mouseinput/mouseinputreference/mouseinputmessages/wm_ncmouseleave.asp
            _chrome.OnNonClientMouseLeave(EventArgs.Empty);
        }

        private void WmNCLButtonDown(ref Message msg)
        {
            var pt = PointToClient(new Point(msg.LParam.ToInt32()));
            var args = new NonClientMouseEventArgs(
                MouseButtons.Left, 1, pt.X, pt.Y, 0, msg.WParam.ToInt32()
            );

            _chrome.OnNonClientMouseDown(args);

            if (!args.Handled)
                base.WndProc(ref msg);

            msg.Result = (IntPtr)1;
        }

        private void WmNCLButtonUp(ref Message msg)
        {
            var pt = PointToClient(new Point(msg.LParam.ToInt32()));
            var args = new NonClientMouseEventArgs(
                MouseButtons.Left, 1, pt.X, pt.Y, 0, msg.WParam.ToInt32()
            );

            _chrome.OnNonClientMouseUp(args);

            if (!args.Handled)
                base.WndProc(ref msg);

            msg.Result = (IntPtr)1;
        }

        private void WmNCPaint(ref Message msg)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/pantdraw_8gdw.asp
            // example in q. 2.9 on http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c41c.aspx#q1026q

            PaintNonClientArea(msg.HWnd, msg.WParam);

            msg.Result = (IntPtr)1;
        }

        private void WmSetText(ref Message msg)
        {
            base.WndProc(ref msg);

            PaintNonClientArea(msg.HWnd, (IntPtr)1);
        }

        private void WmNCActivate(ref Message msg)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowmessages/wm_ncactivate.asp

            if (_form.WindowState == FormWindowState.Minimized)
            {
                base.WndProc(ref msg);
            }
            else
            {
                PaintNonClientArea(msg.HWnd, (IntPtr)1);

                msg.Result = (IntPtr)1;
            }
        }

        public void PaintNonClientArea()
        {
            PaintNonClientArea(Handle, (IntPtr)1);
        }

        private void PaintNonClientArea(IntPtr hWnd, IntPtr hRgn)
        {
            var windowRect = new NativeMethods.RECT();

            if (NativeMethods.GetWindowRect(hWnd, ref windowRect) == 0)
                return;

            var bounds = new Rectangle(
                0,
                0,
                windowRect.right - windowRect.left,
                windowRect.bottom - windowRect.top
            );

            if (bounds.Width == 0 || bounds.Height == 0)
                return;

            // The update region is clipped to the window frame. When wParam
            // is 1, the entire window frame needs to be updated. 
            Region clipRegion = null;

            if (hRgn != (IntPtr)1)
                clipRegion = Region.FromHrgn(hRgn);

            // MSDN states that only WINDOW and INTERSECTRGN are needed,
            // but other sources confirm that CACHE is required on Win9x
            // and you need CLIPSIBLINGS to prevent painting on overlapping windows.

            var hDC = NativeMethods.GetDCEx(
                hWnd,
                hRgn,
                NativeMethods.DCX_WINDOW | NativeMethods.DCX_INTERSECTRGN | NativeMethods.DCX_CACHE | NativeMethods.DCX_CLIPSIBLINGS
            );

            if (hDC == IntPtr.Zero)
                hDC = NativeMethods.GetWindowDC(hWnd);

            if (hDC == IntPtr.Zero)
                return;

            try
            {
                var border = _chrome.AdjustedResizeBorderThickness;

                var clientArea = new Rectangle(
                    border.Left,
                    border.Top + _chrome.CaptionHeight,
                    _form.Width - border.Horizontal,
                    _form.Height - (border.Vertical + _chrome.CaptionHeight)
                );

                if (!_chrome.DoubleBuffered)
                {
                    using (Graphics graphics = Graphics.FromHdc(hDC))
                    {
                        graphics.ExcludeClip(clientArea);

                        // Cliping rect is not cliping rect but actual rectangle.

                        _chrome.OnNonClientAreaPaint(new NonClientPaintEventArgs(
                            graphics,
                            bounds,
                            clipRegion,
                            IsMaximized,
                            IsActive
                        ));
                    }

                    // NOTE: The Graphics object would realease the HDC on Dispose.
                    // So there is no need to call NativeMethods.ReleaseDC(msg.HWnd, hDC);
                    // http://groups.google.pl/groups?hl=pl&lr=&c2coff=1&client=firefox-a&rls=org.mozilla:en-US:official_s&threadm=%23DDSaH7BFHA.3644%40TK2MSFTNGP15.phx.gbl&rnum=15&prev=/groups%3Fq%3DWM_NCPaint%2B%2BGetDCEx%26start%3D10%26hl%3Dpl%26lr%3D%26c2coff%3D1%26client%3Dfirefox-a%26rls%3Dorg.mozilla:en-US:official_s%26selm%3D%2523DDSaH7BFHA.3644%2540TK2MSFTNGP15.phx.gbl%26rnum%3D15
                    // http://groups.google.pl/groups?hl=pl&lr=&c2coff=1&client=firefox-a&rls=org.mozilla:en-US:official_s&threadm=cmo00r%24j9v%241%40mamut1.aster.pl&rnum=1&prev=/groups%3Fq%3DDCX_PARENTCLIP%26hl%3Dpl%26lr%3D%26c2coff%3D1%26client%3Dfirefox-a%26rls%3Dorg.mozilla:en-US:official_s%26selm%3Dcmo00r%2524j9v%25241%2540mamut1.aster.pl%26rnum%3D1
                }
                else
                {
                    // http://www.codeproject.com/csharp/flicker_free.asp
                    // http://www.pinvoke.net/default.aspx/gdi32/BitBlt.html

                    var compatiblehDc = NativeMethods.CreateCompatibleDC(hDC);
                    var compatibleBitmap = NativeMethods.CreateCompatibleBitmap(hDC, bounds.Width, bounds.Height);

                    try
                    {
                        NativeMethods.SelectObject(compatiblehDc, compatibleBitmap);

                        // copy current screen to bitmap
                        // TODO: this is quite slow (80% of this method). Why?
                        NativeMethods.BitBlt(compatiblehDc, 0, 0, bounds.Width, bounds.Height, hDC, 0, 0, NativeMethods.SRCCOPY);

                        using (Graphics g = Graphics.FromHdc(compatiblehDc))
                        {
                            g.ExcludeClip(clientArea);

                            //cliping rect is not cliping rect but actual rectangle
                            _chrome.OnNonClientAreaPaint(new NonClientPaintEventArgs(
                                g,
                                bounds,
                                clipRegion,
                                IsMaximized,
                                IsActive
                            ));
                        }

                        // copy current from bitmap to screen
                        NativeMethods.BitBlt(hDC, 0, 0, bounds.Width, bounds.Height, compatiblehDc, 0, 0, NativeMethods.SRCCOPY);
                    }
                    finally
                    {
                        NativeMethods.DeleteObject(compatibleBitmap);
                        NativeMethods.DeleteDC(compatiblehDc);

                    }
                }
            }
            finally
            {
                NativeMethods.ReleaseDC(Handle, hDC);
            }
        }

        private NonClientHitTest CalculateHitTest(Point location)
        {
            var border = _chrome.AdjustedResizeBorderThickness;

            if (location.X < border.Left)
            {
                if (location.Y < border.Top)
                    return NonClientHitTest.TopLeft;
                if (location.Y >= _form.Height - border.Bottom)
                    return NonClientHitTest.BottomLeft;

                return NonClientHitTest.Left;
            }

            if (location.X >= _form.Width - border.Right)
            {
                if (location.Y < border.Top)
                    return NonClientHitTest.TopRight;
                if (location.Y >= _form.Height - border.Bottom)
                    return NonClientHitTest.BottomRight;

                return NonClientHitTest.Right;
            }

            if (location.Y < border.Top)
                return NonClientHitTest.Top;
            if (location.Y >= _form.Height - border.Bottom)
                return NonClientHitTest.Bottom;

            if (location.Y < _chrome.CaptionHeight + border.Top)
                return NonClientHitTest.Caption;

            return NonClientHitTest.Client;
        }

        public Padding GetBorderThickness()
        {
            var rect = new NativeMethods.RECT(Rectangle.Empty);

            NativeMethods.AdjustWindowRectEx(
                ref rect,
                (uint)NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE),
                false,
                (uint)NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE)
            );

            return new Padding(
                rect.right,
                rect.bottom,
                rect.right,
                rect.bottom
            );
        }

        private void CalculateNonClientAreaSize(ref Rectangle bounds)
        {
            Padding border = _chrome.AdjustedResizeBorderThickness;

            bounds = new Rectangle(
                bounds.Left + border.Left,
                bounds.Top + border.Top + _chrome.CaptionHeight,
                bounds.Width - border.Horizontal,
                bounds.Height - (border.Vertical + _chrome.CaptionHeight)
            );
        }

        private delegate void Action();
    }
}
