using IntegerKeys.ViewModel;
using Sawtooth.Sdk.Net.Client;
using Google.Protobuf;
using Sawtooth.Sdk.Net.Transactions.Families.IntKey;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static ClientStateListResponse.Types;
using Google.Protobuf.Collections;

namespace IntegerKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CommittedKey> Keys { get; set; } = new List<CommittedKey>();
        public List<PendingTransaction> PendingTxns { get; set; } = new List<PendingTransaction>();
        
        private ValidatorClient? client;

        private ValidatorStateEventClient? eventClient;

        private IntKeyTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        private string url;

        private Encoder encoder;

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

            encoder = new Encoder(settings, signer.GetPrivateKey());

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
            try
            {
                //event client
                if (eventClient != null) eventClient.Dispose();//Dispose previous one
                eventClient = ValidatorStateEventClient.Create(url, m => AutoRefresh(m), (e, m) => HandleError(e, m), txnFamily.AddressPrefix);

                if (client != null)
                {
                    client.Dispose();
                }

                client = ValidatorClient.Create(url);

                Keys.Clear();

                await LoadKeysAsync(); //Load All keys

                RefreshUI();


            }catch(Exception e)
            {
                MessageBox.Show("Error: " + e.Message + "[Will retry in 1 seconds]");

                //Resource may not be ready, try again
                await Task.Delay(TimeSpan.FromSeconds(1));
                await FetchDataAsync();
            }
        }

        private void HandleError(ClientEventsSubscribeResponse.Types.Status status, string message)
        {
            MessageBox.Show("Unable to subscribe to state events : " + message + $"({status})");
        }

        private void AutoRefresh(StateChange stateChange)
        {

            if (stateChange.Type == StateChange.Types.Type.Set)
            {
                IntKeyState ik_state = txnFamily.UnwrapStatePayload(stateChange.Value.ToByteArray());
                CommittedKey key = new CommittedKey(ik_state.Name, ik_state.Value);

                Dispatcher.Invoke(() => { AddOrUpdateKeyToList(key); });
            }
            else
            {
                //TODO: handle deletion via address
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
                if (key.Status != ClientBatchStatus.Types.Status.Committed)
                {
                    PendingTxns.Add(key);
                }
            }
            else
            {

                if(key.Status == ClientBatchStatus.Types.Status.Committed)
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

            FullList<Entry> states = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix);
            foreach (var state in states.List)
            {
                if (state?.Data != null)
                {
                    IntKeyState ik_state = txnFamily.UnwrapStatePayload(state.Data.ToByteArray());
                    CommittedKey key = new CommittedKey(ik_state.Name, ik_state.Value);

                    Dispatcher.Invoke(() => { AddOrUpdateKeyToList(key); });
                }
            }

        }

        private async Task<bool> CallIntKeyTxn(string name, string verb, uint value)
        {
            if (client == null) return false;

            IntKeyTransaction txn = txnFamily.CreateEmptyTxn();

            txn.Name = name;
            txn.Verb = verb;
            txn.Value = value;

            try
            {
                var batchIds = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapTxnPayload(txn)));

                //Create pending entries
                Dispatcher.Invoke(() => 
                {
                    foreach (var batchId in batchIds) {
                        AddOrUpdatePendingToList(new PendingTransaction(batchId,txn,ClientBatchStatus.Types.Status.Pending));
                    }
                });

                //Now check status 
                await CheckStatus(txn, batchIds);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return false;
        }

        private async Task CheckStatus(IntKeyTransaction txn, RepeatedField<string> batchIds)
        {
            try
            {
                if (client == null) return;

                var statuses = await client.GetBatchStatusesAsync(batchIds);

                int index = 0;
                foreach (var status in statuses)
                {
                    PendingTransaction pending = new PendingTransaction(status.BatchId, txn, status.Status);

                    if (pending.Status == ClientBatchStatus.Types.Status.Pending)
                    {
                        //Check after sometime
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            await CheckStatus(txn, batchIds);
                        });
                    }
                    else if (pending.Status == ClientBatchStatus.Types.Status.Invalid)
                    {
                        pending.Message = status.InvalidTransactions[index].Message;
                        Dispatcher.Invoke(() => AddOrUpdatePendingToList(pending));
                    }
                    else if (pending.Status == ClientBatchStatus.Types.Status.Committed)
                    {
                        Dispatcher.Invoke(() => AddOrUpdatePendingToList(pending));
                    }
                    index++;
                }
            }
            catch
            {
                //Resource may not be ready, try again
                await Task.Delay(TimeSpan.FromSeconds(1));
                await CheckStatus(txn, batchIds);
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

            uint uintValue = uint.MaxValue;
            try
            {
                uintValue = uint.Parse(value);
            }
            catch
            {
                //ignore
            }

            if(await CallIntKeyTxn(name, "set", uintValue))
            {
                Dispatcher.Invoke(() => { tbVariable.Text = ""; tbValue.Text = ""; });
            } 


        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (eventClient != null) eventClient.Dispose();
            if (client != null) client.Dispose();

        }
    }
}
