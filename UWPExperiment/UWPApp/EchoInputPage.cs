using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Streaming.Adaptive;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TallyWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public class EchoInputPage : Page
    {

        private bool _uiloaded = false;
        private StackPanel vArgsPanel;
        public EchoInputPage()
        {
            LoadUI();
        }
        private void LoadUI()
        {

            if (_uiloaded)
                return;

            Grid grid = new Grid();

            //Margin
            grid.Margin = new Thickness(10, 10, 10, 10);

            //Background
            grid.Background = new SolidColorBrush(Colors.AntiqueWhite);

            //3x3 :  Columns and Rows

            // Define the Columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });

            // Define the Rows
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(250) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

            Border border = new Border();
            Grid.SetColumn(border, 1);
            Grid.SetRow(border, 0);

            TextBlock arguments = new TextBlock { Text = "Arguments" };
            border.Child = arguments;

            grid.Children.Add(border);

            vArgsPanel = new StackPanel { Orientation = Orientation.Vertical };
            Grid.SetColumn(vArgsPanel, 1);
            Grid.SetRow(vArgsPanel, 1);

            int numargs = 5;
            for (int i = 0; i < numargs; i++)
            {
                TextBox arg = new TextBox { Height = Double.NaN, Margin = new Thickness(0, 5, 0, 5) };
                vArgsPanel.Children.Add(arg);
            }

            grid.Children.Add(vArgsPanel);


            StackPanel button_panel = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(button_panel, 1);
            Grid.SetRow(button_panel, 3);

            Button echo = new Button();
            echo.Content = "Echo";
            echo.Click += EchoClicked;
            button_panel.Children.Add(echo);

            Button secho = new Button();
            secho.Content = "Server Echo";
            secho.Margin = new Thickness(20, 0, 0, 0);
            secho.Click += ServerEchoClicked;
            button_panel.Children.Add(secho);


            grid.Children.Add(button_panel);


            Content = grid;

            _uiloaded = true;

        }

        private void EchoClicked(object sender, RoutedEventArgs e)
        {

            string[] args = new string[vArgsPanel.Children.Count];

            for (int i = 0; i < vArgsPanel.Children.Count; i++)
                args[i] = ((TextBox)vArgsPanel.Children[i]).Text;

            //TWXApplication.TWPostEvent(25, 2, "ECHO", args.Length, args);


            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(EchoResponsePage), args);

        }

        private void ServerEchoClicked(object sender, RoutedEventArgs e)
        {

            string[] args = new string[vArgsPanel.Children.Count];

            for (int i = 0; i < vArgsPanel.Children.Count; i++)
                args[i] = ((TextBox)vArgsPanel.Children[i]).Text;

            //TWXApplication.TWPostEvent(25, 3, "ECHO", args.Length, args);

        }
    }
}
