using ProtoBuf;
using System.ComponentModel;


namespace Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf
{
    //State

    [ProtoContract]
    public class Account
    {
        [ProtoMember(1, Name = "customer_id")]
        public uint CustomerId { get; set; }

        [ProtoMember(2, Name = "customer_name")]
        public string? CustomerName { get; set; }

        [ProtoMember(3, Name = "savings_balance")]
        public uint SavingsBalance { get; set; }

        [ProtoMember(4, Name = "checking_balance")]
        public uint CheckingBalance { get; set; }

    }

    //Trasnaction Payload

    [ProtoContract]
    public class SmallbankTransactionPayload
    {
        public enum PayloadType
        {
            CREATE_ACCOUNT = 1,
            DEPOSIT_CHECKING = 2,
            WRITE_CHECK = 3,
            TRANSACT_SAVINGS = 4,
            SEND_PAYMENT = 5,
            AMALGAMATE = 6
        }

        [ProtoMember(1, Name = "payload_type")]
        public PayloadType Type { get; set; }

        [ProtoMember(2, Name = "create_account")]
        public CreateAccountTransactionData? CreateAccount { get; set; }

        [ProtoMember(3, Name = "deposit_checking")]
        public DepositCheckingTransactionData? DepositChecking { get; set; }

        [ProtoMember(4, Name = "write_check")]
        public WriteCheckTransactionData? WriteCheck { get; set; }

        [ProtoMember(5, Name = "transact_savings ")]
        public TransactSavingsTransactionData? TransactSavings { get; set; }

        [ProtoMember(6, Name = "send_payment")] 
        public SendPaymentTransactionData? SendPayment { get; set; }

        [ProtoMember(7, Name = "amalgamate")] 
        public AmalgamateTransactionData? Amalgamate { get; set; }
    }


    [ProtoContract]
    public class CreateAccountTransactionData
    {
        // The CreateAccountTransaction creates an account

        // Customer ID
        [ProtoMember(1, Name = "customer_id")]
        public uint CustomerId { get; set; }

        // Customer Name
        [ProtoMember(2, Name = "customer_name")]
        public string? CustomerName { get; set; }

        // Initial Savings Balance (in cents to avoid float)
        [ProtoMember(3, Name = "initial_savings_balance")]
        public uint InitialSavingsBalance { get; set; }

        // Initial Checking Balance (in cents to avoid float)
        [ProtoMember(4, Name = "initial_checking_balance")] 
        public uint InitialCheckingBalance { get; set; }
    }


    [ProtoContract]
    public class DepositCheckingTransactionData
    {
        // The DepositCheckingTransction adds an amount to the customer's
        // checking account.

        // Customer ID
        [ProtoMember(1, Name = "customer_id")] 
        public uint CustomerId { get; set; }

        // Amount
        [ProtoMember(2, Name = "amount")] 
        public uint Amount { get; set; }
    }


    [ProtoContract]
    public class WriteCheckTransactionData
    {
        // The WriteCheckTransaction removes an amount from the customer's
        // checking account.

        // Customer ID
        [ProtoMember(1, Name = "customer_id")] 
        public uint CustomerId { get; set; }

        // Amount
        [ProtoMember(2, Name = "amount")] 
        public uint Amount { get; set; }
    }


    [ProtoContract]
    public class TransactSavingsTransactionData
    {
        // The TransactSavingsTransaction adds an amount to the customer's
        // savings account. Amount may be a negative number.

        // Customer ID
        [ProtoMember(1, Name = "customer_id")] 
        public uint CustomerId { get; set; }

        // Amount
        [ProtoMember(2, Name = "amount")] 
        public int Amount { get; set; }
    }


    [ProtoContract]
    public class SendPaymentTransactionData
    {
        // The SendPaymentTransaction transfers an amount from one customer's
        // checking account to another customer's checking account.

        // Source Customer ID
        [ProtoMember(1, Name = "source_customer_id")] 
        public uint SourceCustomerId { get; set; }

        // Destination Customer ID
        [ProtoMember(2, Name = "dest_customer_id")] 
        public uint DestCustomerId { get; set; }

        // Amount
        [ProtoMember(3, Name = "amount")] 
        public uint Amount { get; set; }
    }


    [ProtoContract]
    public class AmalgamateTransactionData
    {
        // The AmalgamateTransaction transfers the entire contents of one
        // customer's savings account into another customer's checking
        // account.

        // Source Customer ID
        [ProtoMember(1, Name = "source_customer_id")] 
        public uint SourceCustomerId { get; set; }

        // Destination Customer ID
        [ProtoMember(2, Name = "dest_customer_id")] 
        public uint DestCustomerId { get; set; }
    }
}
