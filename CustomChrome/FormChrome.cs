using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    public class FormChrome : FormComponent
    {
        private static readonly bool _designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private ChromeManager _chromeManager;
        private CornerRadius _cornerRadius;
        private Size _regionSize;
        private bool _maximizedRegion;

        [DefaultValue(0)]
        public int CaptionHeight { get; set; }

        public Padding ResizeBorderThickness { get; set; }

        public bool IsActive
        {
            get { return _chromeManager.IsActive; }
        }

        public bool IsMaximized
        {
            get { return _chromeManager.IsMaximized; }
        }

        [Browsable(false)]
        public Padding AdjustedResizeBorderThickness
        {
            get
            {
                if (AdjustWhenMaximized && _chromeManager != null && _chromeManager.IsMaximized)
                    return _chromeManager.GetBorderThickness();
                else
                    return ResizeBorderThickness;
            }
        }

        public CornerRadius CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                if (_cornerRadius != value)
                {
                    _cornerRadius = value;

                    UpdateFormRegion();
                }
            }
        }

        [DefaultValue(true)]
        public bool AdjustWhenMaximized { get; set; }

        [DefaultValue(false)]
        public bool DoubleBuffered { get; set; }

        public void PaintNonClientArea()
        {
            _chromeManager.PaintNonClientArea();
        }

        public void Invalidate()
        {
            NativeMethods.SendMessage(
                _chromeManager.Handle,
                NativeMethods.WM_NCPAINT,
                (IntPtr)1,
                IntPtr.Zero
            );
        }

        public override Form Form
        {
            get { return base.Form; }
            protected set
            {
                if (Form != value)
                {
                    if (_chromeManager != null)
                    {
                        _chromeManager.ReleaseHandle();
                        _chromeManager = null;
                    }

                    base.Form = value;

                    if (Form != null && !_designMode)
                    {
                        _chromeManager = new ChromeManager(Form, this);

                        if (Form.IsHandleCreated)
                            UpdateFormRegion();
                        else
                            Form.HandleCreated += (s, e) => UpdateFormRegion();
                    }
                }
            }
        }

        public event NonClientPaintEventHandler NonClientAreaPaint;

        protected internal virtual void OnNonClientAreaPaint(NonClientPaintEventArgs e)
        {
            var ev = NonClientAreaPaint;

            if (ev != null)
                ev(this, e);
        }

        public event SystemCommandEventHandler SystemCommand;

        protected internal virtual void OnSystemCommand(SystemCommandEventArgs e)
        {
            var ev = SystemCommand;

            if (ev != null)
                ev(this, e);
        }

        public event MouseEventHandler NonClientMouseMove;

        protected internal virtual void OnNonClientMouseMove(MouseEventArgs e)
        {
            var ev = NonClientMouseMove;

            if (ev != null)
                ev(this, e);
        }

        public event EventHandler NonClientMouseLeave;

        protected internal virtual void OnNonClientMouseLeave(EventArgs e)
        {
            var ev = NonClientMouseLeave;

            if (ev != null)
                ev(this, e);
        }

        public event NonClientMouseEventHandler NonClientMouseDown;

        protected internal virtual void OnNonClientMouseDown(NonClientMouseEventArgs e)
        {
            var ev = NonClientMouseDown;

            if (ev != null)
                ev(this, e);
        }

        public event NonClientMouseEventHandler NonClientMouseUp;

        protected internal virtual void OnNonClientMouseUp(NonClientMouseEventArgs e)
        {
            var ev = NonClientMouseUp;

            if (ev != null)
                ev(this, e);
        }

        public FormChrome()
            : this(null)
        {
        }

        public FormChrome(IContainer container)
        {
            if (container != null)
                container.Add(this);

            AdjustWhenMaximized = true;
        }

        private void UpdateFormRegion()
        {
            if (_designMode)
                return;

            var form = (Form)ContainerControl;

            if (_chromeManager.IsMaximized && AdjustWhenMaximized)
            {
                if (!_maximizedRegion || _regionSize != form.Size)
                {
                    _regionSize = form.Size;
                    _maximizedRegion = true;

                    var border = _chromeManager.GetBorderThickness();

                    form.Region = new Region(new Rectangle(
                        border.Left,
                        border.Top,
                        form.Width - border.Horizontal,
                        form.Height - border.Vertical
                    ));

                    _chromeManager.PaintNonClientArea();
                }

                return;
            }

            if (_maximizedRegion)
            {
                _maximizedRegion = false;
                _regionSize = Size.Empty;
            }

            if (_regionSize == form.Size)
                return;

            _regionSize = form.Size;

            if (
                !double.IsNaN(CornerRadius.All) &&
                CornerRadius.All <= 0
            ) {
                form.Region = null;

                _chromeManager.PaintNonClientArea();

                return;
            }

            GraphicsPath path = new GraphicsPath();

            if (CornerRadius.TopLeft > 0)
            {
                path.AddArc(
                    0,
                    0,
                    CornerRadius.TopLeft * 2,
                    CornerRadius.TopLeft * 2,
                    180,
                    90
                );
            }

            if (CornerRadius.TopRight > 0)
            {
                path.AddArc(
                    form.Width - CornerRadius.TopRight * 2,
                    0,
                    CornerRadius.TopRight * 2,
                    CornerRadius.TopRight * 2,
                    270,
                    90
                );
            }
            else
            {
                path.AddLine(
                    CornerRadius.TopLeft,
                    0,
                    form.Width,
                    0
                );
            }

            if (CornerRadius.BottomRight > 0)
            {
                path.AddArc(
                    form.Width - CornerRadius.BottomRight * 2,
                    form.Height - CornerRadius.BottomRight * 2,
                    CornerRadius.BottomRight * 2,
                    CornerRadius.BottomRight * 2,
                    0,
                    90
                );
            }
            else
            {
                path.AddLine(
                    form.Width,
                    CornerRadius.TopRight,
                    form.Width,
                    form.Height
                );
            }

            if (CornerRadius.BottomLeft > 0)
            {
                path.AddArc(
                    0,
                    form.Height - CornerRadius.BottomLeft * 2,
                    CornerRadius.BottomLeft * 2,
                    CornerRadius.BottomLeft * 2,
                    90,
                    90
                );
            }
            else
            {
                path.AddLine(
                    form.Width - CornerRadius.BottomRight,
                    form.Height,
                    0,
                    form.Height
                );
            }

            path.CloseFigure();

            form.Region = new Region(path);

            _chromeManager.PaintNonClientArea();
        }

        internal void UpdateWindowState()
        {
            UpdateFormRegion();
        }

        public Point PointToClient(Point location)
        {
            return _chromeManager.PointToClient(location);
        }
    }
}
