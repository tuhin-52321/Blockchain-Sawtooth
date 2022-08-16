using Sawtooth.Sdk.Net.Utils;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for GameArea.xaml
    /// </summary>
    public partial class GameArea : UserControl
    {
        private Func<string, int, Task> OnTakeSpace;
        private Func<string, Task> OnDeleteGame;
        private GameContext context;
        public string Player1 => context.Player1;
        public string Player2 => context.Player2;

        public GameArea(string name, string myPlayer, Func<string, int, Task> takeSpace, Func<string, Task> deleteGame)
        {
            InitializeComponent();

            OnTakeSpace = takeSpace;

            OnDeleteGame = deleteGame;

            context = new GameContext(myPlayer);

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

                        ProgressBar progressBar = new ProgressBar();
                        progressBar.IsIndeterminate = true;
                        progressBar.Width = 50;
                        progressBar.Height = 10;
                        SetButtonContent(button, progressBar);
                        DisableAllButtons();
                        await OnTakeSpace(Name, position);
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
            Refresh();
        }

        public bool UpdateGame(string status, string board, string? player1, string? player2)
        {
            if (
                   status.Equals(context.Status) 
                && board.Equals(context.Position.ToString()) 
                && context.Player1.Equals(player1)
                && context.Player2.Equals(player2)
                )
            {
                return false;
            }

            context.AtomicUpdate(status,board, player1, player2);

            UpdateGame();

            return true;
        }

        private void UpdateGame()
        {
            bool isMyTurn = context.IsMyTurn;

            for (int pos = 0; pos < 9; pos++)
            {

                Button b = GetButtonAt(pos);

                char space = context.Position[pos];

                Dispatcher.Invoke(() =>
                {
                    b.IsEnabled = isMyTurn && space == '-';
                    b.Content = space == '-' ? "" : space;
                });
            }


            switch (context.Status)
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

        private void SetButtonContent(Button b, object content)
        {
            Dispatcher.Invoke(() =>
            {
                b.Content = content;
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

        private string _status = "";
        public string Status => _status; 

        private StringBuilder _board = new StringBuilder("");
        public StringBuilder Position => _board;

        public void AtomicUpdate(string status, string board, string? player1, string? player2)
        {
            lock(update_lock)
            {
                _status = status;
                _board = new StringBuilder(board);
                if (!string.IsNullOrEmpty(player1)) this.player1 = player1;
                if (!string.IsNullOrEmpty(player2)) this.player2 = player2;
            }
        }

        public string GameStatus => GameStatusToString(Status);

        private string player1 = "";
        private string player2 = "";
        private string myPlayer;

        public GameContext(string myPlayer)
        {
            this.myPlayer = myPlayer;
        }

        public string Player1Short  => string.IsNullOrEmpty(player1) ? "<Not Joined>" : player1.First(6);
        public string Player2Short  => string.IsNullOrEmpty(player1) ? "<Not Joined>" : player2.First(6); 

        public string Player1 => player1;
        public string Player2 => player2;

        private string GameStatusToString(string status)
        {
            switch (status)
            {
                case "P1-NEXT": return IamPlayer1 ? "Please enter your move (X) ..." : "Waiting for X's turn ...";
                case "P2-NEXT": return IamPlayer2 ? "Please enter your move (O) ..." : "Waiting for O's turn ...";
                case "P1-WIN": return "Player X won.";
                case "P2-WIN": return "Player O won.";
                case "TIE": return "Game tied.";
            }

            return "<Unknown>";
        }

        public bool IsMyTurn
        {
            get
            {
                switch (Status)
                {
                    case "P1-NEXT":
                        {
                            if (string.IsNullOrEmpty(Player1)) return true;
                            if (Player1.Equals(myPlayer)) return true;
                            return false;
                        }
                    case "P2-NEXT":
                        {
                            if (string.IsNullOrEmpty(Player2)) return true;
                            if (Player2.Equals(myPlayer)) return true;
                            return false;
                        }

                }

                return false;
            }
        }

        public bool IamPlayer1
        {
            get
            {
                if (string.IsNullOrEmpty(Player1)) return true;
                if (Player1.Equals(myPlayer)) return true;

                return false;
            }
        }

        public bool IamPlayer2
        {
            get
            {
                if (string.IsNullOrEmpty(Player2)) return true;
                if (Player2.Equals(myPlayer)) return true;

                return false;
            }
        }


    }
}
