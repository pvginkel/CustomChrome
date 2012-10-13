using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text;
using System.Windows.Forms;

namespace CustomChrome
{
    public abstract class FormComponent : Component
    {
        private ContainerControl _containerControl;

        [Browsable(false)]
        public ContainerControl ContainerControl
        {
            get { return _containerControl; }
            set
            {
                if (_containerControl != value)
                {
                    _containerControl = value;

                    Form = _containerControl as Form;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;

                if (value == null)
                    return;

                var host = (IDesignerHost)value.GetService(typeof(IDesignerHost));

                if (host != null)
                    ContainerControl = host.RootComponent as ContainerControl;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Form Form { get; protected set; }
    }
}
