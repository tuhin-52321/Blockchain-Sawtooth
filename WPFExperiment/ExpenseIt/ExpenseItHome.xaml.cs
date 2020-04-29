using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpenseIt
{
    /// <summary>
    /// Interaction logic for ExpenseItHome.xaml
    /// </summary>
    public partial class ExpenseItHome : Page
    {
        public ExpenseItHome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // View Expense Report
            string[] args = new string[5];
            args[0] = this.Arg1.Text;
            args[1] = this.Arg2.Text;
            args[2] = this.Arg3.Text;
            args[3] = this.Arg4.Text;
            args[4] = this.Arg5.Text;
            ExpenseReportPage expenseReportPage = new ExpenseReportPage(args);
            this.NavigationService.Navigate(expenseReportPage);
        }
    }
}
