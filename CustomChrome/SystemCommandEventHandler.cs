using System;
using System.Collections.Generic;
using System.Text;

namespace CustomChrome
{
    public class SystemCommandEventArgs
    {
        public SystemCommand Command { get; private set; }

        public SystemCommandEventArgs(SystemCommand command)
        {
            Command = command;
        }
    }

    public delegate void SystemCommandEventHandler(object sender, SystemCommandEventArgs e);
}
