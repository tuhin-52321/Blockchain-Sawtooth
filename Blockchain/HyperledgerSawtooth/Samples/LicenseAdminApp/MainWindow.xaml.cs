using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
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
using System.Xml.Linq;
using LicenseTransactionProcessor.Tally;
using log4net.Core;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Utilities.Encoders;
using Sawtooth.Sdk.Net.Client;
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
        private ValidatorStateEventClient eventClient;

        private LicenseTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        Sawtooth.Sdk.Net.Client.Encoder encoder;

        public MainWindow(string url, string loginid)
        {
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

            eventClient = ValidatorStateEventClient.Create(url, m => AutoRefresh(m), (e, m) => HandleError(e, m), txnFamily.AddressPrefix);

            client = ValidatorClient.Create(url);

            Title = $"License Admin - {loginid} [{url}]";


            Task.Run(async () =>
            {
                await LoadAllLicenses();
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
        private async Task LoadAllLicenses()
        {
            if (client == null) return;

            try
            {
                var items = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix);
                this.items.Clear();
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
            catch
            {

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
                LicenseState state = txnFamily.UnwrapStatePayload(stateChange.Value.ToByteArray());
                if (state?.Payload != null)
                {
                    VuLicense? license = items.Find(x => x.Id.Equals(state.Payload.LicenseId));
                    if (license == null)
                    {
                        license = new VuLicense(signer.GetPublicKey().ToHexString(), state.Payload);
                        items.Add(license);
                    }
                    else
                    {
                        license.Update(state.Payload);
                    }
                    Refresh();
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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (eventClient != null) eventClient.Dispose();
            if (client != null) client.Dispose();

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
