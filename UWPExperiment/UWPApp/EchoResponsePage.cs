using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TallyWorld
{


    public class EchoResponsePage : Page
    {

        public EchoResponsePage()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            string[] args = e.Parameter as string[];

            Grid grid = new Grid();

            grid.Background = new SolidColorBrush(Colors.AliceBlue);

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

            TextBlock arguments = new TextBlock { Text = "Responses" };
            border.Child = arguments;

            grid.Children.Add(border);

            StackPanel args_panel = new StackPanel { Orientation = Orientation.Vertical };
            Grid.SetColumn(args_panel, 1);
            Grid.SetRow(args_panel, 1);


            for (int i = 0; i < args.Length; i++)
            {
                TextBlock arg = new TextBlock() { Height = Double.NaN, Margin = new Thickness(0, 5, 0, 5) };
                arg.Text = $"{i + 1}. {args[i]}";
                args_panel.Children.Add(arg);
            }

            grid.Children.Add(args_panel);

            StackPanel button_panel = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(button_panel, 1);
            Grid.SetRow(button_panel, 3);

            Button back = new Button();
            back.Content = "< Back";
            back.Click += BackClicked;
            button_panel.Children.Add(back);


            grid.Children.Add(button_panel);

            Content = grid;



        }

        private void BackClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(EchoInputPage));
        }
    }
}
