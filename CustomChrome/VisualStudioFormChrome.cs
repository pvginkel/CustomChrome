using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    public partial class VisualStudioFormChrome : FormComponent
    {
        private static readonly Size IconSize = new Size(24, 24);
        private static readonly Size ButtonSize = new Size(34, 26);
        private static readonly Point ButtonImageOffset = new Point(12, 7);
        private static readonly Point IconOffset = new Point(10, 5);
        private static readonly Point TextOffset = new Point(7, 10);

        private static readonly bool _designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private FormChrome _formChrome;
        private DropShadowManager _dropShadowManager;
        private Color _borderColor;
        private Color _primaryColor;
        private Brush _primaryColorBrush;
        private ImageCacheManager _primaryImageCacheManager = new ImageCacheManager();
        private ImageCacheManager _borderImageCacheManager = new ImageCacheManager();
        private ImageCache _primaryImageCache;
        private ImageCache _blackImageCache;
        private ImageCache _whiteImageCache;
        private ImageCache _grayImageCache;
        private Icon _lastFormIcon;
        private Image _formImage;
        private ChromeButton _overButton;
        private ChromeButton _downButton;
        private Padding _lastBorder;
        private bool _disposed;
        private VisualStudioButton _overExtraButton;
        private VisualStudioButton _downExtraButton;

        private Point? CaptureStart { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Form Form
        {
            get { return base.Form; }
            protected set
            {
                if (Form != value)
                {
                    if (_dropShadowManager != null)
                    {
                        _dropShadowManager.Dispose();
                        _dropShadowManager = null;
                    }

                    if (Form != null)
                    {
                        Form.MouseMove -= Form_MouseMove;
                        Form.MouseUp -= Form_MouseUp;
                    }

                    base.Form = value;

                    if (Form != null && !_designMode)
                    {
                        Form.MouseMove += Form_MouseMove;
                        Form.MouseUp += Form_MouseUp;

                        _formChrome.ContainerControl = Form;

                        _dropShadowManager = new DropShadowManager(Form)
                        {
                            ImageCache = _borderImageCacheManager.GetCached(BorderColor)
                        };
                    }
                }
            }
        }

        [DefaultValue(typeof(Color), "0xFF007ACC")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;

                    if (!_designMode)
                        UpdateBorderColor();
                }
            }
        }

        [DefaultValue(typeof(Color), "0xFF007ACC")]
        public Color PrimaryColor
        {
            get { return _primaryColor; }
            set
            {
                if (_primaryColor != value)
                {
                    _primaryColor = value;

                    if (!_designMode)
                    {
                        _primaryImageCache = _primaryImageCacheManager.GetCached(value);

                        if (Form != null)
                            _formChrome.PaintNonClientArea();

                        if (_primaryColorBrush != null)
                            _primaryColorBrush.Dispose();

                        _primaryColorBrush = new SolidBrush(_primaryColor);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VisualStudioButtonCollection Buttons { get; private set; }

        public VisualStudioFormChrome()
        {
            Buttons = new VisualStudioButtonCollection(this);

            _formChrome = new FormChrome
            {
                CaptionHeight = 31,
                ResizeBorderThickness = new Padding(0),
                DoubleBuffered = true
            };

            _formChrome.NonClientAreaPaint += _formChrome_NonClientAreaPaint;
            _formChrome.NonClientMouseDown += _formChrome_NonClientMouseDown;
            _formChrome.NonClientMouseUp += _formChrome_NonClientMouseUp;
            _formChrome.NonClientMouseLeave += _formChrome_NonClientMouseLeave;
            _formChrome.NonClientMouseMove += _formChrome_NonClientMouseMove;
            _formChrome.SystemCommand += _formChrome_SystemCommand;

            _blackImageCache = new ImageCache(Color.Black);
            _whiteImageCache = new ImageCache(Color.White);
            _grayImageCache = new ImageCache(SystemColors.ControlDark);

            PrimaryColor = Color.FromArgb(0, 122, 204);
            BorderColor = Color.FromArgb(0, 122, 204);
        }

        void Form_MouseMove(object sender, MouseEventArgs e)
        {
            ProcessMouseMove();
        }

        void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (CaptureStart.HasValue && e.Button == MouseButtons.Left)
                ProcessMouseUp();
        }

        void _formChrome_NonClientAreaPaint(object sender, NonClientPaintEventArgs e)
        {
            var border = _formChrome.AdjustedResizeBorderThickness;

            if (border != _lastBorder)
            {
                _lastBorder = border;
                _dropShadowManager.Synchronize();
            }

            UpdateBorderColor();

            e.Graphics.Clear(SystemColors.Control);

            var state = new ButtonStates(this);

            if (state.Close.Draw)
                DrawButton(e.Graphics, state.Close, e.IsMaximized);
            if (state.MaximizeRestore.Draw)
                DrawButton(e.Graphics, state.MaximizeRestore, e.IsMaximized);
            if (state.Minimize.Draw)
                DrawButton(e.Graphics, state.Minimize, e.IsMaximized);

            foreach (var extraButton in state.ExtraButtons)
            {
                DrawButton(e.Graphics, extraButton);
            }

            int leftOffset = IconOffset.X + border.Left;

            if (state.DrawIcon)
            {
                e.Graphics.DrawImage(
                    GetFormIcon(),
                    leftOffset,
                    IconOffset.Y + border.Top
                );

                leftOffset += IconSize.Width;
            }

            leftOffset += TextOffset.X;

            var textBounds = new Rectangle(
                leftOffset,
                TextOffset.Y + border.Top,
                state.RightOffset - leftOffset,
                int.MaxValue
            );

            TextRenderer.DrawText(
                e.Graphics,
                Form.Text,
                SystemFonts.MessageBoxFont,
                textBounds,
                SystemColors.ControlDarkDark,
                SystemColors.Control,
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
            );
        }

        private void DrawButton(Graphics graphics, ButtonState button)
        {
            button.ExtraButton.OnPaint(new VisualStudioButtonPaintEventArgs(
                button.ExtraButton,
                graphics,
                button.Bounds,
                null,
                false,
                false
            ));
        }

        private void DrawButton(Graphics graphics, ButtonState button, bool isMaximized)
        {
            graphics.FillRectangle(GetButtonBackgroundBrush(button.State), button.Bounds);

            ImageCache imageCache;

            switch (button.State)
            {
                case ChromeButtonState.Disabled: imageCache = _grayImageCache; break;
                case ChromeButtonState.Down: imageCache = _whiteImageCache; break;
                case ChromeButtonState.Over: imageCache = _primaryImageCache; break;
                default: imageCache = _blackImageCache; break;
            }

            Image image;

            switch (button.Button)
            {
                case ChromeButton.Minimize: image = imageCache.Minimize; break;
                case ChromeButton.MaximizeRestore: image = isMaximized ? imageCache.Restore : imageCache.Maximize; break;
                default: image = imageCache.Close; break;
            }

            graphics.DrawImageUnscaled(
                image,
                button.Bounds.Left + ButtonImageOffset.X,
                button.Bounds.Top + ButtonImageOffset.Y
            );
        }

        internal Brush GetButtonBackgroundBrush(ChromeButtonState state)
        {
            switch (state)
            {
                case ChromeButtonState.Down: return _primaryColorBrush;
                case ChromeButtonState.Over: return Brushes.White;
                default: return SystemBrushes.Control;
            }
        }

        internal Color GetForeColor(bool enabled, ChromeButtonState state)
        {
            if (!enabled)
                return SystemColors.ControlDark;

            switch (state)
            {
                case ChromeButtonState.Over: return _primaryColor;
                case ChromeButtonState.Down: return Color.White;
                default: return Color.Black;
            }
        }

        internal Color GetBackColor(bool enabled, ChromeButtonState state)
        {
            if (!enabled)
                return SystemColors.Control;

            switch (state)
            {
                case ChromeButtonState.Over: return Color.White;
                case ChromeButtonState.Down: return _primaryColor;
                default: return SystemColors.Control;
            }
        }

        void _formChrome_NonClientMouseDown(object sender, NonClientMouseEventArgs e)
        {
            var state = new ButtonStates(this);

            if (state.OverButton != ChromeButton.None || state.OverExtraButton != null)
            {
                _formChrome.BeginUpdate();

                SetOverButton(ChromeButton.None);
                SetOverButton(null);
                SetDownButton(state.OverButton);
                SetDownButton(state.OverExtraButton);

                CaptureStart = e.Location;
                Form.Capture = true;

                _formChrome.EndUpdate();
            }
        }

        void _formChrome_NonClientMouseMove(object sender, MouseEventArgs e)
        {
            ProcessMouseMove();
        }

        private void ProcessMouseMove()
        {
            var state = new ButtonStates(this);

            _formChrome.BeginUpdate();

            SetOverButton(state.OverButton);
            SetOverButton(state.OverExtraButton);
            SetDownButton(state.DownButton);
            SetDownButton(state.DownExtraButton);

            _formChrome.EndUpdate();
        }

        void _formChrome_NonClientMouseUp(object sender, NonClientMouseEventArgs e)
        {
            if (!CaptureStart.HasValue)
                return;

            ProcessMouseUp();
        }

        private void ProcessMouseUp()
        {
            switch (_downButton)
            {
                case ChromeButton.Minimize:
                    Form.WindowState = FormWindowState.Minimized;
                    break;
                case ChromeButton.MaximizeRestore:
                    Form.WindowState = Form.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    break;
                case ChromeButton.Close:
                    Form.Close();
                    break;
            }

            if (_downExtraButton != null)
                _downExtraButton.OnClick(EventArgs.Empty);

            Form.Capture = false;
            CaptureStart = null;

            var state = new ButtonStates(this);

            _formChrome.BeginUpdate();

            SetOverButton(state.OverButton);
            SetDownButton(state.DownButton);
            SetOverButton(state.OverExtraButton);
            SetDownButton(state.DownExtraButton);

            _formChrome.EndUpdate();
        }

        void _formChrome_NonClientMouseLeave(object sender, EventArgs e)
        {
            if (!CaptureStart.HasValue)
                SetOverButton(ChromeButton.None);
        }

        private void SetOverButton(ChromeButton button)
        {
            if (_overButton != button)
            {
                _overButton = button;
                _overExtraButton = null;
                _formChrome.PaintNonClientArea();
            }
        }

        private void SetDownButton(ChromeButton button)
        {
            if (_downButton != button)
            {
                _downButton = button;
                _downExtraButton = null;
                _formChrome.PaintNonClientArea();
            }
        }

        private void SetOverButton(VisualStudioButton extraButton)
        {
            if (_overExtraButton != extraButton)
            {
                _overButton = ChromeButton.None;
                _overExtraButton = extraButton;
                _formChrome.PaintNonClientArea();
            }
        }

        private void SetDownButton(VisualStudioButton extraButton)
        {
            if (_downExtraButton != extraButton)
            {
                _downButton = ChromeButton.None;
                _downExtraButton = extraButton;
                _formChrome.PaintNonClientArea();
            }
        }

        void _formChrome_SystemCommand(object sender, SystemCommandEventArgs e)
        {
        }

        private void UpdateBorderColor()
        {
            if (_dropShadowManager == null)
                return;

            _dropShadowManager.ImageCache =
                _formChrome.IsActive
                ? _borderImageCacheManager.GetCached(_borderColor)
                : _borderImageCacheManager.GetCached(Color.FromArgb(153, 153, 153));
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_borderImageCacheManager != null)
                {
                    _borderImageCacheManager.Dispose();
                    _borderImageCacheManager = null;
                }

                if (_primaryImageCacheManager != null)
                {
                    _primaryImageCacheManager.Dispose();
                    _primaryImageCacheManager = null;
                }

                if (_formChrome != null)
                {
                    _formChrome.Dispose();
                    _formChrome = null;
                }

                if (_dropShadowManager != null)
                {
                    _dropShadowManager.Dispose();
                    _dropShadowManager = null;
                }

                if (_blackImageCache != null)
                {
                    _blackImageCache.Dispose();
                    _blackImageCache = null;
                }

                if (_whiteImageCache != null)
                {
                    _whiteImageCache.Dispose();
                    _whiteImageCache = null;
                }

                if (_grayImageCache != null)
                {
                    _grayImageCache.Dispose();
                    _grayImageCache = null;
                }

                if (_primaryColorBrush != null)
                {
                    _primaryColorBrush.Dispose();
                    _primaryColorBrush = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private Image GetFormIcon()
        {
            if (Form.Icon != _lastFormIcon)
            {
                _lastFormIcon = Form.Icon;

                if (_formImage != null)
                {
                    _formImage.Dispose();
                    _formImage = null;
                }

                if (Form.Icon != null)
                {
                    using (var icon = new Icon(Form.Icon, IconSize))
                    {
                        _formImage = icon.ToBitmap();
                    }
                }
            }

            return _formImage;
        }

        public void PaintNonClientArea()
        {
            _formChrome.PaintNonClientArea();
        }
    }
}
