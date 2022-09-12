using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LicenseUserApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 2)
            {
                MessageBox.Show("Usage: LicenseUserApp <url> <login>");
                Console.WriteLine("Usage: LicenseUserApp <url> <login>");
                Current.Shutdown();
            }
            else
            {
                MainWindow wnd = new MainWindow(e.Args[0], e.Args[1]);
                wnd.Show();
            }
        }
    }
}
