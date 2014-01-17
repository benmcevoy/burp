using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace Burp.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mutex;
        private readonly string _mutexName = "{4876C6CB-5DF8-4EEC-9A82-A53DB7FB7F59}";

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isOwned;

            _mutex = new Mutex(true, _mutexName, out isOwned);
            
            if (!isOwned)
            {
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);
        }
    }
}
