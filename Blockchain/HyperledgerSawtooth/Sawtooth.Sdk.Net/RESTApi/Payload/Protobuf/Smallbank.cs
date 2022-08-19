namespace Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf
{
    [ProtoBuf.ProtoContract()]
    public partial class Account : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension? __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"customer_id")]
        public uint CustomerId { get; set; }

        [ProtoBuf.ProtoMember(2, Name = @"customer_name")]
        [System.ComponentModel.DefaultValue("")]
        public string CustomerName { get; set; } = "";

        [ProtoBuf.ProtoMember(3, Name = @"savings_balance")]
        public uint SavingsBalance { get; set; }

        [ProtoBuf.ProtoMember(4, Name = @"checking_balance")]
        public uint CheckingBalance { get; set; }

    }

    [ProtoBuf.ProtoContract()]
    public partial class SmallbankTransactionPayload : ProtoBuf.IExtensible
    {
        internal static SmallbankTransactionPayload CreateAccountTransactionPayload(uint customerId, string customerName, uint initialSavingsBalance, uint initialCheckingBalance)
        {
            SmallbankTransactionPayload payload = new SmallbankTransactionPayload();

            payload.TxnType = PayloadType.CreateAccount;

            payload.CreateAccount = new CreateAccountTransactionData(customerId, customerName, initialSavingsBalance, initialCheckingBalance);

            return payload;
        }

        private ProtoBuf.IExtension? __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        private SmallbankTransactionPayload() 
        { 
        }

        [ProtoBuf.ProtoMember(1, Name = @"payload_type")]
        public PayloadType TxnType { get; private set; }

        [ProtoBuf.ProtoMember(2, Name = @"create_account")]
        public CreateAccountTransactionData? CreateAccount { get; private set; }

        [ProtoBuf.ProtoMember(3, Name = @"deposit_checking")]
        public DepositCheckingTransactionData? DepositChecking { get; private set; }

        [ProtoBuf.ProtoMember(4, Name = @"write_check")]
        public WriteCheckTransactionData? WriteCheck { get; private set; } 

        [ProtoBuf.ProtoMember(5, Name = @"transact_savings")]
        public TransactSavingsTransactionData? TransactSavings { get; private set; }

        [ProtoBuf.ProtoMember(6, Name = @"send_payment")]
        public SendPaymentTransactionData? SendPayment { get; private set; }

        [ProtoBuf.ProtoMember(7, Name = @"amalgamate")]
        public AmalgamateTransactionData? Amalgamate { get; private set; }


        [ProtoBuf.ProtoContract()]
        public partial class CreateAccountTransactionData : ProtoBuf.IExtensible
        {

            private ProtoBuf.IExtension? __pbn__extensionData;

            public CreateAccountTransactionData()
            {
                CustomerId = 0;
                CustomerName = string.Empty;
                InitialSavingsBalance = 0;
                InitialCheckingBalance = 0;
            }

            public CreateAccountTransactionData(uint customerId, string customerName, uint initialSavingsBalance, uint initialCheckingBalance)
            {
                CustomerId = customerId;
                CustomerName = customerName;
                InitialSavingsBalance = initialSavingsBalance;
                InitialCheckingBalance = initialCheckingBalance;
            }

            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"customer_id")]
            public uint CustomerId { get; private set; }

            [ProtoBuf.ProtoMember(2, Name = @"customer_name")]
            public string CustomerName { get; private set; }

            [ProtoBuf.ProtoMember(3, Name = @"initial_savings_balance")]
            public uint InitialSavingsBalance { get; private set; }

            [ProtoBuf.ProtoMember(4, Name = @"initial_checking_balance")]
            public uint InitialCheckingBalance { get; private set; }
        }

        [ProtoBuf.ProtoContract()]
        public partial class DepositCheckingTransactionData : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension? __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"customer_id")]
            public uint CustomerId { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"amount")]
            public uint Amount { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public partial class WriteCheckTransactionData : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension? __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"customer_id")]
            public uint CustomerId { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"amount")]
            public uint Amount { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public partial class TransactSavingsTransactionData : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension? __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"customer_id")]
            public uint CustomerId { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"amount")]
            public int Amount { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public partial class SendPaymentTransactionData : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension? __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"source_customer_id")]
            public uint SourceCustomerId { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"dest_customer_id")]
            public uint DestCustomerId { get; set; }

            [ProtoBuf.ProtoMember(3, Name = @"amount")]
            public uint Amount { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public partial class AmalgamateTransactionData : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension? __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"source_customer_id")]
            public uint SourceCustomerId { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"dest_customer_id")]
            public uint DestCustomerId { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public enum PayloadType
        {
            [ProtoBuf.ProtoEnum(Name = @"PAYLOAD_TYPE_UNSET")]
            PayloadTypeUnset = 0,
            [ProtoBuf.ProtoEnum(Name = @"CREATE_ACCOUNT")]
            CreateAccount = 1,
            [ProtoBuf.ProtoEnum(Name = @"DEPOSIT_CHECKING")]
            DepositChecking = 2,
            [ProtoBuf.ProtoEnum(Name = @"WRITE_CHECK")]
            WriteCheck = 3,
            [ProtoBuf.ProtoEnum(Name = @"TRANSACT_SAVINGS")]
            TransactSavings = 4,
            [ProtoBuf.ProtoEnum(Name = @"SEND_PAYMENT")]
            SendPayment = 5,
            [ProtoBuf.ProtoEnum(Name = @"AMALGAMATE")]
            Amalgamate = 6,
        }

    }

}