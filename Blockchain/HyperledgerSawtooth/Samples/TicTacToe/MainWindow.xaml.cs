using Google.Protobuf.Collections;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.Transactions.Families.XO;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ValidatorClient client;
        private ValidatorStateEventClient eventClient;

        private XOTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        private ConcurrentDictionary<string, GameArea> games = new ConcurrentDictionary<string, GameArea>();

        Sawtooth.Sdk.Net.Client.Encoder encoder;

        //private SawtoothWSClient websocket;

        public MainWindow(string name, string url)
        {
            InitializeComponent();

            txnFamily = new XOTransactionFamily();

            signer = new Signer(Encoding.UTF8.GetBytes(name));

            settings = new EncoderSettings()
            {
                BatcherPublicKey = signer.GetPublicKey().ToHexString(),
                SignerPublickey = signer.GetPublicKey().ToHexString(),
                FamilyName = txnFamily.Name,
                FamilyVersion = txnFamily.Version
            };
            settings.Inputs.Add(txnFamily.AddressPrefix);
            settings.Outputs.Add(txnFamily.AddressPrefix);

            encoder = new Sawtooth.Sdk.Net.Client.Encoder(settings, signer.GetPrivateKey());

            eventClient = ValidatorStateEventClient.Create(url, m => AutoRefresh(m), (e, m) => HandleError(e, m), txnFamily.AddressPrefix);

            client = ValidatorClient.Create(url);

            Title = $"TicTacToe Client - {name} {url}";


            Task.Run(async () =>
            {
                await LoadAllGames();
            });
        }

        private void HandleError(ClientEventsSubscribeResponse.Types.Status status, string message)
        {
            MessageBox.Show("Unable to subscribe to state events : " + message + $"({status})");
        }

        private void AutoRefresh(StateChange stateChange)
        {

            if (stateChange.Type == StateChange.Types.Type.Set)
            {
                XOState xo_state = txnFamily.UnwrapStatePayload(stateChange.Value.ToByteArray());

                if (xo_state.Name != null)
                {
                    if (!games.TryGetValue(xo_state.Name, out var area))
                    {
                        area = CreateNewGame(xo_state.Name);
                        SetStatus($"New game {xo_state.Name} created.");
                    }
                    if (area != null && xo_state.Status != null && xo_state.Board != null)
                    {
                        if (area.UpdateGame(xo_state.Status, xo_state.Board, xo_state.Player1, xo_state.Player2))
                            SetStatus($"Game {xo_state.Name} updated.");

                    }
                }
            }
            else
            {
                //TODO: handle deletion via address
            }
        }
        private async Task LoadAllGames()
        {
            if (client == null) return;

            try
            {
                var items = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix);
                List<string> current_list = new List<string>();
                foreach (var item in items.List)
                {
                    if (item?.Data != null)
                    {
                        XOState xo_state = txnFamily.UnwrapStatePayload(item.Data.ToByteArray());

                        if (xo_state.Name != null)
                        {
                            if(!games.TryGetValue(xo_state.Name, out var area))
                            {
                                area = CreateNewGame(xo_state.Name);
                                SetStatus($"New game {xo_state.Name} created.");
                            }
                            if (area != null && xo_state.Status != null && xo_state.Board != null)
                            {
                                if(area.UpdateGame(xo_state.Status, xo_state.Board, xo_state.Player1, xo_state.Player2))
                                SetStatus($"Game {xo_state.Name} updated.");

                            }
                            current_list.Add(xo_state.Name);
                        }
                    }
                }
                List<string> to_be_deleted = new List<string>();
                foreach(string prev_game in games.Keys)
                {
                    if (!current_list.Contains(prev_game))
                    {
                        to_be_deleted.Add(prev_game);
                    }
                }
                foreach (string game in to_be_deleted)
                {

                    if (games.TryRemove(game, out _))
                    {
                        DeleteTab(game);
                        SetStatus($"Game {game} deleted.");
                    }
                }
            }
            catch
            {

            }

        }

        private async Task CheckStatus(XOTransaction txn, RepeatedField<string> batchIds)
        {
            if (client == null) return;

            var statuses = await client.GetBatchStatusesAsync(batchIds);

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (status.InvalidTransactions.Count > 0)
                    {
                        SetStatus(status.InvalidTransactions[0]?.Message);
                    }
                    if (status.Status == ClientBatchStatus.Types.Status.Pending)
                    {
                        //Check after sometime
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            await CheckStatus(txn, batchIds);
                        });
                    }
                }
            }
        }


        private void DeleteTab(string name)
        {
            Dispatcher.Invoke(() =>
            {
                TabItem? game_area = null;
                foreach (TabItem item in tcGameArea.Items)
                {
                    if (item.Content is GameArea area && area.Name.Equals(name))
                    {
                        game_area = item;
                        break;
                    }
                }
                if (game_area != null)
                {
                    tcGameArea.Items.Remove(game_area);
                }
            });
        }

        private async Task<bool> CallXOTxn(string name, string action, int space = 1)
        {
            XOTransaction txn = txnFamily.CreateEmptyTxn();

            txn.Name = name;
            txn.Action = action;
            txn.Space = space;

            try
            {
                var batchIds = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapTxnPayload(txn)));

                await CheckStatus(txn, batchIds);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return false;
        }
        private async void CreateNewGame(object sender, RoutedEventArgs e)
        {
            string name = Dispatcher.Invoke(() => { return tbName.Text; });
            if (!string.IsNullOrEmpty(name))
            {
                SetStatus($"Creating new game '{name}' ...");

                if(await CallXOTxn(name, "create"))
                {
                    SetStatus($"Creating new game '{name}' (waiting) ...");
                }
            }
        }

        private async Task TakeSpace(string name, int pos)
        {
            if (client == null) return;

            if(await CallXOTxn(name, "take", pos+1))
            {
            };

        }

        private async Task DeleteGame(string name)
        {
            if (client == null) return;

            SetStatus($"Deleting game {name} ...");

            if (await CallXOTxn(name, "delete"))
            {
                SetStatus($"Wating on game {name} to be deleted.");
            };

        }

        private void SetStatus(string? text)
        {
            Dispatcher.Invoke(() => lStatus.Content = text);
        }

        private GameArea? CreateNewGame(string name)
        {
            return Dispatcher.Invoke(() =>
            {
                var area = new GameArea(name, signer.GetPublicKey().ToHexString(), TakeSpace, DeleteGame);
                if (games.TryAdd(name, area))
                {
                    var item = new TabItem
                    {
                        Header = name,
                        Content = area
                    };

                    tcGameArea.Items.Add(item);

                    tcGameArea.SelectedItem = item;

                    return area;
                }
                return null;
            });

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            eventClient.Dispose();
            client.Dispose();

        }
    }
}
