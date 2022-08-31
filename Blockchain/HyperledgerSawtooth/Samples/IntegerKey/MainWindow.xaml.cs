using IntegerKeys.ViewModel;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using Sawtooth.Sdk.Net.Test.RESTApi.WebSocket;
using Sawtooth.Sdk.Net.Transactions.Families.IntKey;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace IntegerKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CommittedKey> Keys { get; set; } = new List<CommittedKey>();
        public List<PendingTransaction> PendingTxns { get; set; } = new List<PendingTransaction>();
        
        private SawtoothClient? client;

        private SawtoothWSClient? websocket;

        private IntKeyTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        private string url;

        private Sawtooth.Sdk.Net.Client.Encoder encoder;

        public MainWindow(string url)
        {
            InitializeComponent();

            txnFamily = new IntKeyTransactionFamily();

            signer = new Signer();

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

            this.url = url;

            Title = $"IntegerKey Client - {url}";

            Task.Run(async () =>
            {
                await FetchDataAsync();
            });

        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }


        private async Task FetchDataAsync()
        {

            client = new SawtoothClient(url);

            Keys.Clear();

            await LoadKeysAsync(); //Load All keys

            RefreshUI();


            //Websocket client
            if (websocket != null) websocket.Dispose();//Dispose previous one
            Uri uri = new Uri(url);
            websocket = new SawtoothWSClient($"ws://{uri.Host}:{uri.Port}/subscriptions", e => AutoRefresh(e), txnFamily.AddressPrefix);

        }

        private void AutoRefresh(WSEvent? e)
        {
            if(e != null)
            {
                foreach(var state in e.StateChanges)
                {
                    if (state.Value != null)
                    {
                        if (state.Type == "SET")
                        {
                            IntKeyState ik_state = txnFamily.UnwrapStatePayload(state.Value);
                            CommittedKey key = new CommittedKey(ik_state.Name, ik_state.Value);

                            Dispatcher.Invoke(() => { AddOrUpdateKeyToList(key); });
                        }
                        else
                        {
                            //TODO: handle deletion via address
                        }
                    }
                }
            }
        }

        private void AddOrUpdateKeyToList(CommittedKey key)
        {
            CommittedKey? currentKey = Keys.Find(k => k.Name != null && k.Name.Equals(key.Name));
            if (currentKey == null)
            {
                Keys.Add(key);
            }
            else
            {
                currentKey.Value = key.Value;
            }


            RefreshUI();

        }
        private void AddOrUpdatePendingToList(PendingTransaction key)
        {
            PendingTransaction? pending = PendingTxns.Find(k => k.TxnId != null && k.TxnId.Equals(key.TxnId));
            if (pending == null)
            {
                if (key.Status != "COMMITTED")
                {
                    PendingTxns.Add(key);
                }
            }
            else
            {

                if(key.Status == "COMMITTED")
                {
                    PendingTxns.Remove(pending);
                }
                else
                {
                    pending.Status = key.Status;
                    pending.Message = key.Message;

                }
            }

            RefreshUI();

        }


        private void RefreshUI()
        {
            Dispatcher.Invoke(() =>
            {
                DataContext = null;
                DataContext = this;
            });
        }

        private async Task LoadKeysAsync()
        {
            if (client == null) return;

            FullList<StateItem> states = await client.GetStatesWithFilterAsync(txnFamily.AddressPrefix);
            foreach (var state in states.List)
            {
                if (state?.Data != null)
                {
                    IntKeyState ik_state = txnFamily.UnwrapStatePayload(state.Data);
                    CommittedKey key = new CommittedKey(ik_state.Name, ik_state.Value);

                    Dispatcher.Invoke(() => { AddOrUpdateKeyToList(key); });
                }
            }

        }

        private async Task<bool> CallIntKeyTxn(string name, string verb, int value)
        {
            if (client == null) return false;

            IntKeyTransaction txn = txnFamily.CreateEmptyTxn();

            txn.Name = name;
            txn.Verb = verb;
            txn.Value = value;

            try
            {
                var response = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapTxnPayload(txn)));

                if (response != null && response.Link != null)
                {
                    await CheckStatus(txn, response.Link);

                    return true;
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return false;
        }

        private async Task CheckStatus( IntKeyTransaction txn, string link)
        {
            if (client == null) return;

            var statuses = await client.GetBatchStatusUsingLinkAsync(link);

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (status.Id != null && status.Status != null)
                    {
                        PendingTransaction pending = new PendingTransaction(status.Id, txn, status.Status);
                        if (status.InvalidTransaction.Count > 0)
                        {
                            pending.Message = status.InvalidTransaction[0]?.Message;
                        }
                        Dispatcher.Invoke(() => AddOrUpdatePendingToList(pending));

                        if (pending.Status == "PENDING")
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

        private async void IncValueAsync(object sender, RoutedEventArgs e)
        {

            CommittedKey? key = Dispatcher.Invoke(() => { return (sender as FrameworkElement)?.Tag as CommittedKey; });

            if (key == null) return;

            await CallIntKeyTxn(key.Name, "inc", 1); //increment by 1

        }
        private async void DecValueAsync(object sender, RoutedEventArgs e)
        {
            CommittedKey? key = Dispatcher.Invoke(() => { return (sender as FrameworkElement)?.Tag as CommittedKey; });

            if (key == null) return;

            await CallIntKeyTxn(key.Name, "dec", 1); //decrement by 1

        }
 

        private void Value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Value_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private async void SetValue(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Please fetch data first.");
                return;
            }

            string name  = Dispatcher.Invoke(() => { return tbVariable.Text; });
            string value = Dispatcher.Invoke(() => { return tbValue.Text; });

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please set a name.");
                return;
            }
            if (string.IsNullOrEmpty(value)) 
            {
                MessageBox.Show("Please set a value.");
                return;
            }

            int intValue = int.Parse(value);

            if(await CallIntKeyTxn(name, "set", intValue))
            {
                Dispatcher.Invoke(() => { tbVariable.Text = ""; tbValue.Text = ""; });
            } 


        }
    }
}
