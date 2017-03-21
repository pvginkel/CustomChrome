using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CustomChrome.Demo.Properties;

namespace CustomChrome.Demo
{
    public partial class MainForm : Form
    {
        private static readonly Point ButtonImageOffset = new Point(12, 7);

        public MainForm()
        {
            InitializeComponent();

            visualStudioFormChrome1.Buttons.Add(CreateButton(1));
            visualStudioFormChrome1.Buttons.Add(CreateButton(2));
            visualStudioFormChrome1.Buttons.Add(CreateButton(3));
        }

        private VisualStudioButton CreateButton(int index)
        {
            var button = new VisualStudioButton
            {
                Tag = index,
                Enabled = index != 1
            };

            button.Paint += button_Paint;
            button.Click += button_Click;

            return button;
        }

        void button_Click(object sender, EventArgs e)
        {
            var button = (VisualStudioButton)sender;

            if ((int)button.Tag == 2)
                visualStudioFormChrome1.Buttons[2].Visible = !visualStudioFormChrome1.Buttons[2].Visible;
        }

        void button_Paint(object sender, VisualStudioButtonPaintEventArgs e)
        {
            var button = (VisualStudioButton)sender;

            e.PaintBackground();

            using (var image = GetImage(button, e))
            {
                e.Graphics.DrawImageUnscaled(
                    image,
                    e.Bounds.Left + ButtonImageOffset.X,
                    e.Bounds.Top + ButtonImageOffset.Y
                );
            }
        }

        private Bitmap GetImage(VisualStudioButton button, VisualStudioButtonPaintEventArgs e)
        {
            var image = button.Enabled ? Resources.active : Resources.inactive;
            return ImageUtil.GetImage(image, e.ForeColor);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (visualStudioFormChrome1.ContainerControl == null)
                visualStudioFormChrome1.ContainerControl = this;
            else
                visualStudioFormChrome1.ContainerControl = null;
        }
    }
}
