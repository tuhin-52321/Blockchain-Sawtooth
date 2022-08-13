using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IntegerKey
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 1)
            {
                MessageBox.Show("Usage: IntegerKey <url>");
                Console.WriteLine("Usage: IntegerKey <url>");
                Current.Shutdown();
            }
            MainWindow wnd = new MainWindow(e.Args[0]);
            wnd.Show();
        }
    }
}
