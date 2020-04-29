using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExpenseIt_NetCore_CSOnly
{
    internal class ExpenseReportPage  : Page
    {
        private string[] vArgs;

        public ExpenseReportPage(string[] args)
        {
            this.vArgs = args;

            InitializeComponent();
        }


        private void InitializeComponent()
        {
            Title = "ExpenseIt - View Expense";

            Grid grid = new Grid();

            grid.Background = Brushes.AliceBlue;

            //3x3 :  Columns and Rows

            // Define the Columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });

            // Define the Rows
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(300) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

             Border border = new Border();
            Grid.SetColumn(border, 1);
            Grid.SetRow(border, 0);

            Label arguments = new Label { Content = "Responses" };
            border.Child = arguments;

            grid.Children.Add(border);

            StackPanel args_panel = new StackPanel { Orientation = Orientation.Vertical };
            Grid.SetColumn(args_panel, 1);
            Grid.SetRow(args_panel, 1);

            
            for (int i = 0; i < vArgs.Length; i++)
            {
                Label arg = new Label() { Height = Double.NaN, Margin = new Thickness(0, 5, 0, 5) };
                arg.Content = $"{i+1}. {vArgs[i]}";
                args_panel.Children.Add(arg);
            }

            grid.Children.Add(args_panel);

            Content = grid;
        }      

    }
}