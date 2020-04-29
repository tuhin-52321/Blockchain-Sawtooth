using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ExpenseIt_NetCore_CSOnly
{
    public partial class ExpenseItHome : Page
    {

        private StackPanel vArgsPanel;

        public ExpenseItHome()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "ExpenseIt - Home";

            Grid grid = new Grid();

            //Margin
            grid.Margin = new Thickness(10, 10, 10, 10);

            //Background
            grid.Background = Brushes.AntiqueWhite;

            //3x3 :  Columns and Rows

            // Define the Columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });

            // Define the Rows
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(200) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

            Border border = new Border();
            Grid.SetColumn(border, 1);
            Grid.SetRow(border, 0);

            Label arguments = new Label { Content = "Arguments" };
            border.Child = arguments;

            grid.Children.Add(border);

            vArgsPanel = new StackPanel { Orientation = Orientation.Vertical };
            Grid.SetColumn(vArgsPanel, 1);
            Grid.SetRow(vArgsPanel, 1);

            int numargs = 5;
            for (int i = 0; i < numargs; i++) {
                TextBox arg = new TextBox { Height = Double.NaN, Margin = new Thickness(0, 5, 0, 5) };
                vArgsPanel.Children.Add(arg);
            }

            grid.Children.Add(vArgsPanel);


            StackPanel button_panel = new StackPanel { Orientation = Orientation.Vertical };
            Grid.SetColumn(button_panel, 1);
            Grid.SetRow(button_panel, 3);

            Button echo = new Button();
            echo.Content = "Echo";
            echo.Click += EchoClicked;
            button_panel.Children.Add(echo);

            Button secho = new Button();
            secho.Content = "Server Echo";
            //secho.Click += ServerEchoClicked;
            button_panel.Children.Add(secho);


            grid.Children.Add(button_panel);


            Content = grid;
       }

        private void EchoClicked(object sender, RoutedEventArgs e)
        {
 
            string[] args = new string[vArgsPanel.Children.Count];

            for (int i = 0; i < vArgsPanel.Children.Count; i++)
                args[i] = ((TextBox)vArgsPanel.Children[i]).Text;
            
            ExpenseReportPage expenseReportPage = new ExpenseReportPage(args);
            this.NavigationService.Navigate(expenseReportPage);
        }
    }
}