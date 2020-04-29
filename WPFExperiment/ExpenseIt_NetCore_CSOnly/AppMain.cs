using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ExpenseIt_NetCore_CSOnly
{
    public class App : System.Windows.Application
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();

            app.Run(new MainWindow());
        }

    }
}