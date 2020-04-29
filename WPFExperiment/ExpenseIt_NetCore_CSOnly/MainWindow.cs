using System;
using System.Windows;
using System.Windows.Navigation;

namespace ExpenseIt_NetCore_CSOnly
{
    internal class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title  = "ExpenseIt";
            Height = 450;
            Width  = 600;
            Content = new ExpenseItHome();
      
        }
    }
}