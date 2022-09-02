using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Client;
using SawtoothBrowser.Utils;
using SawtoothBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Sawtooth.Sdk.Net.Utils;
using Google.Protobuf;
using Sawtooth.Sdk.Net.Transactions.Families.IntKey;
using Sawtooth.Sdk.Net.Transactions.Families.XO;
using Sawtooth.Sdk.Net.Transactions.Families.Smallbank;

namespace SawtoothBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SawtoothBlock> Blocks { get; set; } = new List<SawtoothBlock>();
        public List<SawtoothBatch> Batches { get; set; } = new List<SawtoothBatch>();
        public List<SawtoothTransaction> Transactions { get; set; } = new List<SawtoothTransaction>();

        public string BlockHeader { get; set; } = "Blocks";
        public string BatchHeader { get; set; } = "Batches";
        public string TxnHeader { get; set; } = "Txn";
        public string TxnDetailHeader { get; set; } = "Txn Detail";

        public string? TxnId { get; set; }
        public string? TxnFamily { get; set; }
        public string? TxnVersion { get; set; }
        public string? TxnPayload { get; set; }

        ValidatorClient? client;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }


        private async void SubmitAsync(object sender, RoutedEventArgs e)
        {
            DataContext = null;
            try
            {
                if(client != null)
                {
                    client.Dispose();
                    client = null;
                }
                client = ValidatorClient.Create(tbUrl.Text);

                Blocks.Clear();

                await LoadBlocksAsync(null); //load first page

                DataContext = this;
            }catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private async Task LoadBlocksAsync(string? start)
        {
            if (client == null) return;

            PageOf<Block> blocks = await client.GetBlocksAsync(start); 

            if (blocks.Next != null) 
                Dispatcher.Invoke(new Action(() => { tbStartBlock.Text = blocks.Next; bBlocksLoadMore.IsEnabled = true; }));
            else
                Dispatcher.Invoke(new Action(() => { tbStartBlock.Text = null; bBlocksLoadMore.IsEnabled = false; }));

            string block_id = blocks.HeadId;

            foreach (var block in blocks.List)
            {
                SawtoothBlock viewBlock = new SawtoothBlock(block_id, block);

                Blocks.Add(viewBlock);
                block_id = viewBlock.PrevBlockId;
            }

            BlockHeader = $"Blocks({Blocks.Count})";

            tabControl.SelectedItem = tabBlocks;
        }

        private async void BlocksLoadMoreAsync(object sender, RoutedEventArgs e)
        {
            await LoadBlocksAsync(Dispatcher.Invoke(() => { return tbStartBlock.Text; })); //load next page
        }

        private async void ShowBatchesOfBlockAsync(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            Batches.Clear();

            DataContext = null;

            string? blockId = (sender as FrameworkElement)?.Tag as string;

            if (blockId != null)
            {
                var block = Blocks.Find(x => x.BlockId == blockId);
                if (block != null)
                {
                    List<ClientBatchStatus> batchStatuses = await client.GetBatchStatusesAsync(block.Header.BatchIds);
                    int index = 0;
                    foreach (var batch in block.Block.Batches)
                    {
                        ClientBatchStatus? status = batchStatuses.Find(x => x.BatchId == block.Header.BatchIds[index]);
                        if (status != null)
                        {
                            SawtoothBatch stbatch = new SawtoothBatch(block.Header.BatchIds[index], batch, status);
                            Batches.Add(stbatch);
                        }
                        index++;
                    }
                    BatchHeader = $"Batch of Block {block.BlockNum}({Batches.Count})";
                }
            }


            DataContext = this;

            tabControl.SelectedItem = tabBatches;
        }

        private async void ShowTrasnactionsOfBatchAsync(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            Transactions.Clear();

            DataContext = null;

            string? batchId = (sender as FrameworkElement)?.Tag as string;

            if (batchId != null)
            {
                var batch = Batches.Find(x => x.BatchId == batchId);
                if (batch != null)
                {
                    List<ClientBatchStatus> batchStatuses = await client.GetBatchStatusAsync(batch.BatchId);
                    int index = 0;
                    foreach (var txn in batch.Batch.Transactions)
                    {
                        SawtoothTransaction st_txn = new SawtoothTransaction(batch.Header.TransactionIds[index], txn, batchStatuses[0]);
                        Transactions.Add(st_txn);
                        index++;
                    }
                    TxnHeader = $"Txns of Batch {batch.BatchIdShort}({Transactions.Count})";
                }
            }


            DataContext = this;

            tabControl.SelectedItem = tabTxn;

        }

        private async void ShowTxnDetailAsync(object sender, RoutedEventArgs e)
        {
            if (client == null) return;

            TxnId = null;
            TxnFamily = null;
            TxnVersion = null;
            TxnPayload = null;

            DataContext = null;

            string? txnId = (sender as FrameworkElement)?.Tag as string;

            if (txnId != null)
            {
                TxnId = txnId;
                Transaction? txnDetail = await client.GetTransactionAsync(txnId);
                if (txnDetail != null)
                {
                    TransactionHeader txnHeader = new TransactionHeader();
                    txnHeader.MergeFrom(txnDetail.Header);

                    TxnFamily = txnHeader.FamilyName;
                    TxnVersion = txnHeader.FamilyVersion;
                    if ("intkey".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                    {
                        TxnPayload = new IntKeyTransactionFamily().UnwrapTxnPayload(txnDetail.Payload.ToByteArray()).DisplayString;
                    }
                    else if ("sawtooth_settings".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                    {
                        TxnPayload = new SawtoothSettingsTransactionFamily().UnwrapTxnPayload(txnDetail.Payload.ToByteArray()).DisplayString;
                    }
                    else if ("xo".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                    {
                        TxnPayload = new XOTransactionFamily().UnwrapTxnPayload(txnDetail.Payload.ToByteArray()).DisplayString;
                    }
                    else if ("smallbank".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                    {
                        TxnPayload = new SmallbankTransactionFamily().UnwrapTxnPayload(txnDetail.Payload.ToByteArray()).DisplayString;
                    }
                    else
                    {
                        TxnPayload = "[Raw data: ]\n" + txnDetail.Payload;
                    }
                }
                else
                {
                    TxnPayload = "<Txn Not Found>";
                }
                TxnDetailHeader = $"Txn Detail: {TxnId.Shorten(16)}";
                
            }
            DataContext = this;

            tabControl.SelectedItem = tabTxnDetail;
        }
    }
}
