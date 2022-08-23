using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using Sawtooth.Sdk.Net.Test.RESTApi.WebSocket;
using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Concurrent;
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
using State = Sawtooth.Sdk.Net.RESTApi.Payload.Json.State;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SawtoothClient client;


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

            client = new SawtoothClient(url);

            Title = $"TicTacToe Client - {name} {url}";

            //Uri uri = new Uri(url);
            //websocket = new SawtoothWSClient($"ws://{uri.Host}:{uri.Port}/subscriptions", e => Task.Run(RefreshAllGames), txnFamily.AddressPrefix);

            Task.Run(async () =>
            {
                while(true)
                {
                    await RefreshAllGames();
                    await Task.Delay(1000);
                }
            });
        }

        private async Task RefreshAllGames()
        {
            if (client == null) return;

            try
            {
                var items = await client.GetStatesWithFilterAsync(txnFamily.AddressPrefix);
                List<string> current_list = new List<string>();
                foreach (var item in items.List)
                {
                    if (item?.Data != null)
                    {
                        XOState xo_state = txnFamily.UnwrapStatePayload(item.Data);

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

        private async Task CheckStatus(XOTransaction txn, string link)
        {
            if (client == null) return;

            var statuses = await client.GetBatchStatusUsingLinkAsync(link);

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (status.Id != null && status.Status != null)
                    {
                        if (status.InvalidTransaction.Count > 0)
                        {
                            SetStatus(status.InvalidTransaction[0]?.Message);
                        }
                        if (status.Status == "PENDING")
                        {
                            //Check after sometime
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                await CheckStatus(txn, link);
                            });
                        }
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
                var response = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapTxnPayload(txn)));

                if (response != null && response.Link != null)
                {
                    await CheckStatus(txn, response.Link);

                    return true;
                }
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
    }
}
