using Google.Protobuf.Collections;
using LicenseTransactionProcessor.Tally;
using Sawtooth.Sdk.Net.Client;
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

namespace LicenseUserApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ValidatorClient client;

        private LicenseTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        Sawtooth.Sdk.Net.Client.Encoder encoder;

        public MainWindow(string url, string loginid)
        {
            InitializeComponent();

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

            client = ValidatorClient.Create(url);

            Title = $"User App - {loginid} [{url}]";

            Task.Run(async () =>
            {
                await ValidateLicense();
            });

        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (client != null) client.Dispose();

        }

        private void OnValidate(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            Task.Run(async () =>
            {
                await ValidateLicense();
            });
        }

        private async Task ValidateLicense()
        {
            try
            {
                var items = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix);
                License? assigned_license = null;
                foreach (var item in items.List)
                {
                    if (item?.Data != null)
                    {
                        LicenseState state = txnFamily.UnwrapStatePayload(item.Data.ToByteArray());
                        License license = state.Payload;
                        if (license.AssignedTo.Equals(signer.GetPublicKey().ToHexString()))
                        {
                            assigned_license = license;
                        }
                    }
                }
                if (assigned_license != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        tbStatus.Text = assigned_license.Type.ToString();
                        bAssign.IsEnabled = false;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        tbStatus.Text = "Unassigned";
                        bAssign.IsEnabled = true;
                    });
                }
            }
            catch
            {

            }

        }

        private void OnAssign(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            var assignLicense = new AssignLicense();
            if (assignLicense.ShowDialog() == true)
            {
                Task.Run(async () =>
                {
                    await AssignLicenseTxn(assignLicense.Type);
                });
            }

        }

        private async Task AssignLicenseTxn(LicenseType type)
        {
            LicenseTransaction txn = LicenseTransaction.AssignLicenseTransaction(type, signer.GetPublicKey().ToHexString());

            try
            {
                var batchIds = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txn.Wrap()));
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
                    if (status.Status == ClientBatchStatus.Types.Status.Committed)
                    {
                        await ValidateLicense();
                    }
                }
            }
        }
    }
}
