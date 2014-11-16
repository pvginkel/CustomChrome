using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    public class VisualStudioButton
    {
        private bool _enabled;
        private bool _visible;

        internal VisualStudioFormChrome Chrome { get; set; }

        public event VisualStudioButtonPaintEventHandler Paint;

        internal protected virtual void OnPaint(VisualStudioButtonPaintEventArgs e)
        {
            var ev = Paint;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler Click;

        protected internal virtual void OnClick(EventArgs e)
        {
            var ev = Click;
            if (ev != null)
                ev(this, e);
        }

        public bool IsOver { get; internal set; }

        public bool IsDown { get; internal set; }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (Chrome != null)
                        Chrome.PaintNonClientArea();
                }
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    if (Chrome != null)
                        Chrome.PaintNonClientArea();
                }
            }
        }

        public object Tag { get; set; }

        public VisualStudioButton()
        {
            Visible = true;
            Enabled = true;
        }
    }
}
