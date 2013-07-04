﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    internal class DropShadowManager : NativeWindow, IDisposable
    {
        private DropShadow[] _controls;
        private bool _disposed;
        private readonly Form _form;
        private ImageCache _imageCache;

        public ImageCache ImageCache
        {
            get { return _imageCache; }
            set
            {
                if (_imageCache != value)
                {
                    _imageCache = value;

                    if (_controls != null)
                    {
                        foreach (var control in _controls)
                        {
                            control.ImageCache = _imageCache;
                        }

                        Synchronize();
                    }
                }
            }
        }

        public DropShadowManager(Form form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            _form = form;

            if (form.IsHandleCreated)
            {
                AssignHandle(form.Handle);
                InitializeForm();
            }
            else
            {
                form.HandleCreated += form_HandleCreated;
            }

            form.HandleDestroyed += form_HandleDestroyed;
        }

        void form_HandleCreated(object sender, EventArgs e)
        {
            AssignHandle(_form.Handle);
            InitializeForm();
        }

        void form_HandleDestroyed(object sender, EventArgs e)
        {
            ReleaseHandle();
        }

        private void InitializeForm()
        {
            _controls = new[]
            {
                new DropShadow(_form, DropShadowBorder.Left)
                {
                    ImageCache = _imageCache
                },
                new DropShadow(_form, DropShadowBorder.Top)
                {
                    ImageCache = _imageCache
                },
                new DropShadow(_form, DropShadowBorder.Right)
                {
                    ImageCache = _imageCache
                },
                new DropShadow(_form, DropShadowBorder.Bottom)
                {
                    ImageCache = _imageCache
                }
            };

            Synchronize();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_WINDOWPOSCHANGED:
                    var windowPos = (NativeMethods.WINDOWPOS)m.GetLParam(typeof(NativeMethods.WINDOWPOS));

                    Synchronize(new Rectangle(
                        windowPos.x,
                        windowPos.y,
                        windowPos.cx,
                        windowPos.cy
                    ));
                    break;
            }

            base.WndProc(ref m);
        }

        public void Synchronize()
        {
            Synchronize(_form.Bounds);
        }

        private void Synchronize(Rectangle bounds)
        {
            if (_controls == null)
                return;

            for (int i = 0; i < _controls.Length; i++)
            {
                _controls[i].Synchronize(bounds);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_controls != null)
                {
                    foreach (var control in _controls)
                    {
                        control.Dispose();
                    }

                    _controls = null;
                }

                _form.HandleCreated -= form_HandleCreated;
                _form.HandleDestroyed -= form_HandleDestroyed;

                ReleaseHandle();

                _disposed = true;
            }
        }
    }
}
