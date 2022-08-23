﻿using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.RESTApi.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using SawtoothBrowser.Utils;
using SawtoothBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Sawtooth.Sdk.Net.Utils;

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

        SawtoothClient? client;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }


        private async void SubmitAsync(object sender, RoutedEventArgs e)
        {
            DataContext = null;

            client = new SawtoothClient(tbUrl.Text);

            Blocks.Clear();

            await LoadBlocksAsync(null); //load first page

            DataContext = this;

        }

        private async Task LoadBlocksAsync(string? start)
        {
            if (client == null) return;

            PageOf<Block> blocks = await client.GetBlocksAsync(start); 

            if (blocks.Next != null) 
                Dispatcher.Invoke(new Action(() => { tbStartBlock.Text = blocks.Next; bBlocksLoadMore.IsEnabled = true; }));
            else
                Dispatcher.Invoke(new Action(() => { tbStartBlock.Text = null; bBlocksLoadMore.IsEnabled = false; }));

            string? block_id = blocks.Head;

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
                    List<BatchStatus>? batchStatuses = null;
                    string?[]? batch_ids = block.Block?.Header?.BatchIds.ToArray();
                    if (batch_ids != null)
                    {
                        batchStatuses = await client.GetBatchStatusesAsync(batch_ids);
                        List<Batch?>? batches = block.Block?.Batches;
                        if (batches != null)
                        {
                            int index = 0;
                            foreach (var batch in batches)
                            {
                                SawtoothBatch stbatch = new SawtoothBatch(batch_ids[index], batch, batchStatuses?.Find(x => x.Id == batch_ids[index]));
                                Batches.Add(stbatch);
                                index++;
                            }
                        }
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
                    List<BatchStatus>? batchStatuses = null;
                    batchStatuses = await client.GetBatchStatusesAsync(batch.BatchId);
                    List<Transaction?>? txns = batch.Batch?.Transactions;
                    if (txns != null)
                    {
                        int index = 0;
                        foreach (var txn in txns)
                        {
                            SawtoothTransaction st_txn = new SawtoothTransaction(batch.Batch?.Header?.TransactionIds[index], txn, batchStatuses?[0]);
                            Transactions.Add(st_txn);
                            index++;
                        }
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

            if(txnId != null)
            {
                TxnId = txnId;
                var txnDetail = await client.GetTransactionAsync(txnId);
                if (txnDetail != null)
                {
                    TxnFamily = txnDetail.Header?.FamilyName;
                    TxnVersion = txnDetail.Header?.FamilyVersion;
                    if (txnDetail.Payload != null)
                    {
                        if ("intkey".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                        {
                            TxnPayload = new IntKeyTransactionFamily().UnwrapTxnPayload(txnDetail.Payload).DisplayString;
                        }
                        else if ("sawtooth_settings".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                        {
                            TxnPayload = new SawtoothSettingsTransactionFamily().UnwrapTxnPayload(txnDetail.Payload).DisplayString;
                        }
                        else if ("xo".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                        {
                            TxnPayload = new XOTransactionFamily().UnwrapTxnPayload(txnDetail.Payload).DisplayString;
                        }
                        else if ("smallbank".Equals(TxnFamily) && "1.0".Equals(TxnVersion))
                        {
                            TxnPayload = new SmallbankTransactionFamily().UnwrapTxnPayload(txnDetail.Payload).DisplayString;
                        }
                        else
                        {
                            TxnPayload = "[Raw data: ]\n" + txnDetail.Payload;
                        }
                    }
                    else
                    {
                        TxnPayload = "<Null Value>";
                    }
                }
                TxnDetailHeader = $"Txn Detail: {TxnId.Shorten(16)}";
            }

            DataContext = this;

            tabControl.SelectedItem = tabTxnDetail;
        }
    }
}
