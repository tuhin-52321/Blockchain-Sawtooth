using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Policy;
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
using System.Windows.Threading;
using System.Xml.Linq;
using Google.Protobuf.Collections;
using LicenseTransactionProcessor.Tally;
using log4net;
using log4net.Core;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Utilities.Encoders;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.Processor;
using Sawtooth.Sdk.Net.Transactions.Families.XO;
using Sawtooth.Sdk.Net.Utils;

namespace LicenseAdminApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<VuLicense> items = new List<VuLicense>();

        private ValidatorClient client;

        private LicenseTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        Sawtooth.Sdk.Net.Client.Encoder encoder;

        readonly string url;

        public MainWindow(string url, string loginid)
        {
            this.url = url;

            InitializeComponent();

            lvLicenses.ItemsSource = items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvLicenses.ItemsSource);
            view.Filter = UserFilter;

            txnFamily = new LicenseTransactionFamily();

            signer = new Signer(Encoding.UTF8.GetBytes(loginid)); 

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

            client = ValidatorClient.Create(url, AutoRefresh);

            Title = $"License Admin - {loginid} [{url}]";


            DisableUI();

            Task.Run(async () =>
            {
                try
                {
                    await client.SubscribeStateChangeEvents(m => AutoRefresh(m), txnFamily.AddressPrefix);
                    await LoadAllLicenses();
                }
                finally
                {
                    EnableUI();
                }
            });
        }

        private async Task RefreshAsync()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                CollectionViewSource.GetDefaultView(lvLicenses.ItemsSource).Refresh();
            });
        }
        private void Refresh()
        {
            Dispatcher.Invoke(() =>
            {
                CollectionViewSource.GetDefaultView(lvLicenses.ItemsSource).Refresh();
            });
        }
        private void DisableUI()
        {
            Dispatcher.Invoke(() =>
            {
                panel.IsEnabled = false;
            });
        }
        private void EnableUI()
        {
            Dispatcher.Invoke(() =>
            {
                panel.IsEnabled = true;
            });
        }

        private async Task LoadAllLicenses()
        {
            if (client == null) return;

            try
            {
                this.items.Clear();
                await RefreshAsync();
                var items = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix, 30); //Expect 30 seconds
                foreach (var item in items.List)
                {
                    if (item?.Data != null)
                    {
                        var state = txnFamily.UnwrapStatePayload(item.Data.ToByteArray());
                        var license = new VuLicense(signer.GetPublicKey().ToHexString(), state.Payload);
                        this.items.Add(license);
                    }

                }
                await RefreshAsync();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

            }
        }

        private void HandleError(ClientEventsSubscribeResponse.Types.Status status, string message)
        {
            MessageBox.Show("Unable to subscribe to state events : " + message + $"({status})");
        }

        private void UpdateLicenseItem(License license)
        {
            VuLicense? vulicense = items.Find(x => x.Id.Equals(license.LicenseId));
            if (vulicense == null)
            {
                vulicense = new VuLicense(signer.GetPublicKey().ToHexString(), license);
                items.Add(vulicense);
            }
            else
            {
                vulicense.Update(license);
            }
            Refresh();
        }

        private void AutoRefresh(StateChange stateChange)
        {

            if (stateChange.Type == StateChange.Types.Type.Set)
            {
                LicenseState state = txnFamily.UnwrapStatePayload(stateChange.Value.ToByteArray());
                if (state?.Payload != null)
                {
                    UpdateLicenseItem(state.Payload);
                }
            }
            else
            {
                //TODO: handle deletion via address
            }
        }
        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return
                        (item as VuLicense)?.Id.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                     || (item as VuLicense)?.Assignee.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                     || (item as VuLicense)?.Status.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                     || (item as VuLicense)?.Type.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    ;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lvLicenses.ItemsSource).Refresh();
        }
        private void OnCreate(object sender, RoutedEventArgs e)
        {

            LicenseTransaction[] transactions = new LicenseTransaction[(int)tbNumbers.Value];
            for (int i = 0; i < transactions.Length; i++)
            {
                transactions[i] = LicenseTransaction.CreateLicenseTransaction(Guid.NewGuid().ToString(), rbGold.IsChecked == true ? LicenseType.Gold : LicenseType.Silver, signer.GetPublicKey().ToHexString());
            }

            Task.Run(async () =>
            {
                await CallLicenseTxn(transactions);

                AutoRefresh();
            });
        }

        private void OnApprove(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            string? licenseId = (sender as FrameworkElement)?.Tag as string;

            if (licenseId != null)
            {
                Task.Run(async () =>
                {
                    await CallLicenseTxn(new LicenseTransaction[] { LicenseTransaction.ApproveLicenseTransaction(licenseId,signer.GetPublicKey().ToHexString()) });

                    AutoRefresh();

                });
            }
        }

        private void OnUnassign(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            string? assignee = (sender as FrameworkElement)?.Tag as string;

            if (assignee != null)
            {
                Task.Run(async () =>
                {
                    await CallLicenseTxn(new LicenseTransaction[] { LicenseTransaction.UnassignLicenseTransaction(assignee) });

                    AutoRefresh();

                });
            }
        }

        private async Task CallLicenseTxn(LicenseTransaction[] transactions)
        {
            Transaction[] txns = new Transaction[transactions.Length];
            for(int i=0;i < transactions.Length; i++)
            {
                txns[i] = encoder.CreateTransaction(transactions[i].Wrap());
            }
            try
            {
                var batchIds = await client.PostBatchListAsync(encoder.Encode(encoder.CreateBatch(txns)));
                await CheckStatus(batchIds);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private async Task CheckStatus(RepeatedField<string> batchIds)
        {
            if (client == null) return;

            var statuses = await client.GetBatchStatusesAsync(batchIds);

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (status.InvalidTransactions.Count > 0)
                    {
                        MessageBox.Show(status.InvalidTransactions[0]?.Message);
                    }
                    if (status.Status == ClientBatchStatus.Types.Status.Pending)
                    {
                        //Check after sometime
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            await CheckStatus(batchIds);
                        });
                    }
                }
            }
        }


        private void OnClosed(object sender, EventArgs e)
        {
            if (client != null)
            {
                _ = client.UnsubscribeFromAllEvents();
                client.Dispose();
            }
        }

        private void AutoRefresh()
        {
            DisableUI();

            _ = client.UnsubscribeFromAllEvents();
            client.Dispose();

            client = ValidatorClient.Create(url, AutoRefresh);

            Task.Run(async () =>
            {
                try
                {
                    await client.SubscribeStateChangeEvents(m => AutoRefresh(m), txnFamily.AddressPrefix);
                    await LoadAllLicenses();
                }
                finally
                {
                    EnableUI();
                }
            });
        }
    }
    public class VuLicense
    {
        private string me;
        private License license;

        public VuLicense(string me, License license)
        {
            this.me = me;
            this.license = license;
        }

        public string Id => license.LicenseId;
        public string Type => license.Type.ToString();
        public string Status => !string.IsNullOrEmpty(license.AssignedTo) ? "Assigned" : !string.IsNullOrEmpty(license.ApprovedBy) ? "Approved" : "Created";
        public string Assignee => license.AssignedTo;

        public string AssigneeShort => !string.IsNullOrEmpty(license.AssignedTo) ? Assignee.First(6) + "..." : "<Not Assigned>";

        public bool CanApprove => string.IsNullOrEmpty(license.ApprovedBy) && !me.Equals(license.CreatedBy);
        public bool CanUnassign => !string.IsNullOrEmpty(license.AssignedTo);

        public void Update(License lic)
        {
            license = lic;
        }
    }
}
