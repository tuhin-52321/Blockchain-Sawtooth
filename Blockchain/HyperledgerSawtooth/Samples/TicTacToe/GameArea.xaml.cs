using Sawtooth.Sdk.Net.Utils;
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

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for GameArea.xaml
    /// </summary>
    public partial class GameArea : UserControl
    {
        private Func<string, int,Task> OnTakeSpace;
        private Func<string, Task> OnDeleteGame;
        private GameContext context;
        public string Player1 => context.Player1;
        public string Player2 => context.Player2;

        public GameArea(string name, Func<string, int, Task> takeSpace, Func<string, Task> deleteGame)
        {
            InitializeComponent();

            OnTakeSpace = takeSpace;

            OnDeleteGame = deleteGame;

            context = new GameContext();

            DataContext = context;
            Name = name;
        }

        private void Refresh()
        {
            Dispatcher.Invoke(() =>
            {
                DataContext = null;
                DataContext = context;
            });
        }

        private async void TakeSpace(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if(button != null)
            {
                object tag = Dispatcher.Invoke(() => { return button.Tag; });
                if(tag is string position_string)
                {
                    try
                    {
                        int position = int.Parse(position_string);
                        await OnTakeSpace(Name, position);
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
        }

        public void UpdateGame(string status, string board, string? player1, string? player2)
        {
            context.AtomicUpdate(status,board, player1, player2);

            UpdateGame();
        }

        private void UpdateGame()
        {
            for (int pos = 0; pos < 9; pos++)
            {

                Button b = GetButtonAt(pos);

                if (context.Position[pos] == '-')
                {
                    Dispatcher.Invoke(() =>
                    {
                        b.IsEnabled = true;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        b.IsEnabled = false;
                        b.Content = context.Position[pos];
                    });

                }
            }


            switch (context.StatusValue)
            {
                case "P1-WIN":
                case "P2-WIN":
                case "TIE": DisableAllButtons(); break;
            }


            Refresh();
        }

        private Button GetButtonAt(int pos)
        {
            if (pos == 0) return b0;
            if (pos == 1) return b1;
            if (pos == 2) return b2;
            if (pos == 3) return b3;
            if (pos == 4) return b4;
            if (pos == 5) return b5;
            if (pos == 6) return b6;
            if (pos == 7) return b7;
            return b8;
        }

        private void DisableAllButtons()
        {
            Dispatcher.Invoke(() =>
            {
                b0.IsEnabled = false;
                b1.IsEnabled = false;
                b2.IsEnabled = false;
                b3.IsEnabled = false;
                b4.IsEnabled = false;
                b5.IsEnabled = false;
                b6.IsEnabled = false;
                b7.IsEnabled = false;
                b8.IsEnabled = false;
            });
        }

        private async void DeleteGame(object sender, RoutedEventArgs e)
        {
            await OnDeleteGame(Name);
        }
    }

    public class GameContext
    {
        private object update_lock = new object();

        private string _status = "P1-NEXT";
        public string StatusValue => _status; 

        private StringBuilder _board = new StringBuilder("---------");
        public StringBuilder Position => _board;

        public void AtomicUpdate(string status, string board, string? player1, string? player2)
        {
            lock(update_lock)
            {
                _status = status;
                _board = new StringBuilder(board);
                if (!string.IsNullOrEmpty(player1)) Player1 = player1.First(6);
                if (!string.IsNullOrEmpty(player2)) Player2 = player2.First(6);
            }
        }

        public string Status => GameStatusToString(StatusValue);

        public string Player1 { get; internal set; } = "<Not Joined>";
        public string Player2 { get; internal set; } = "<Not Joined>";


        public string Position0 => (Position[0] == '-')?" ":Position[0]+"";
        public string Position1 => (Position[1] == '-')?" ":Position[1]+"";
        public string Position2 => (Position[2] == '-')?" ":Position[2]+"";
        public string Position3 => (Position[3] == '-')?" ":Position[3]+"";
        public string Position4 => (Position[4] == '-')?" ":Position[4]+"";
        public string Position5 => (Position[5] == '-')?" ":Position[5]+"";
        public string Position6 => (Position[6] == '-')?" ":Position[6]+"";
        public string Position7 => (Position[7] == '-')?" ":Position[7]+"";
        public string Position8 => (Position[8] == '-')?" ":Position[8]+"";

        private static string GameStatusToString(string status)
        {
            switch (status)
            {
                case "P1-NEXT": return "Waiting for Player 1's turn.";
                case "P2-NEXT": return "Waiting for Player 2's turn.";
                case "P1-WIN": return "Player 1 won.";
                case "P2-WIN": return "Player 2 won.";
                case "TIE": return "Game tied.";
            }

            return "<Unknown>";
        }

    }
}
