using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CustomChrome
{
    [SuppressUnmanagedCodeSecurity]
    [ComVisible(false)]
    internal static class NativeMethods
    {
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_CLIPSIBLINGS = 0x04000000;
        public const int WS_CLIPCHILDREN = 0x02000000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_SYSMENU = 0x00080000;

        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_LAYERED = 0x00080000;

        public const int ULW_ALPHA = 0x02;

        public const byte AC_SRC_OVER = 0x00;
        public const byte AC_SRC_ALPHA = 0x01;

        public const int SC_CLOSE = 0xF060;

        public const int MF_BYCOMMAND = 0x0;
        public const int MF_GRAYED = 0x1;
        public const int MF_DISABLED = 0x2;

        public const int SRCCOPY = 0x00CC0020;

        public const int GWL_STYLE = (-16);
        public const int GWL_EXSTYLE = (-20);

        public const int DCX_CACHE = 0x2;
        public const int DCX_CLIPSIBLINGS = 0x10;
        public const int DCX_INTERSECTRGN = 0x80;
        public const int DCX_WINDOW = 0x1;

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;

        public const int RDW_INVALIDATE = 0x0001;
        public const int RDW_UPDATENOW = 0x0100;
        public const int RDW_FRAME = 0x0400;

        public const int WM_SETTEXT = 0x000C;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_NCCALCSIZE = 0x0083;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_NCACTIVATE = 0x0086;
        public const int WM_NCMOUSEMOVE = 0x00A0;
        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int WM_NCUAHDRAWCAPTION = 0x00AE;
        public const int WM_NCLBUTTONUP = 0x00A2;
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;
        public const int WM_NCRBUTTONDOWN = 0x00A4;
        public const int WM_NCRBUTTONUP = 0x00A5;
        public const int WM_NCRBUTTONDBLCLK = 0x00A6;
        public const int WM_NCMBUTTONDOWN = 0x00A7;
        public const int WM_NCMBUTTONUP = 0x00A8;
        public const int WM_NCMBUTTONDBLCLK = 0x00A9;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_NCMOUSELEAVE = 0x02A2;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(Point point)
            {
                x = point.X;
                y = point.Y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(Size size)
            {
                cx = size.Width;
                cy = size.Height;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(Rectangle r)
            {
                left = r.Left;
                top = r.Top;
                right = r.Right;
                bottom = r.Bottom;
            }

            public Rectangle ToRectangle()
            {
                return new Rectangle(left, top, right - left, bottom - top);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hWndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        //http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowstructures/nccalcsize_params.asp
        [StructLayout(LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS
        {
            public RECT rectProposed;
            public RECT rectBeforeMove;
            public RECT rectClientBeforeMove;
            public WINDOWPOS lpPos;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int Offset);

        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr rectUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

        [DllImport("uxtheme.dll")]
        public static extern int SetWindowTheme(IntPtr hwnd, String pszSubAppName, String pszSubIdList);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern uint GetMenuState(IntPtr hMenu, uint uId, uint uFlags);
    }
}
