using ProtoBuf;
using Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf;
using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class SmallbankTransactionFamily : TransactionFamily<SmallbankState, SmallbankTransaction>
    {

        public SmallbankTransactionFamily() : base("smallbank", "1.0")
        {

        }

        public override string AddressPrefix => Encoding.UTF8.GetBytes("smallbank").ToSha512().ToHexString().First(6);

        public override string AddressSuffix(string context)
        {
            return Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().Last(64);

        }

    }

    public class SmallbankState : ProtobufPayload<Account>, IState
    {

        public string DisplayString {
            get
            {
                string buffer = "[Protobuf Object: Account]\n";
                if (Payload != null)
                {
                    buffer += "{\n";
                    buffer += $"    Customer Id      = {Payload.CustomerId}\n";
                    buffer += $"    Customer Name    = {Payload.CustomerName}\n";
                    buffer += $"    Savings Balance  = {Payload.SavingsBalance}\n";
                    buffer += $"    Checking Balance = {Payload.CheckingBalance}\n";
                    buffer += "}\n";

                }
                else
                {
                    buffer += "<Null Value>";
                }

                return buffer;
            }
        }


        public string AddressContext => Payload?.CustomerId!=null? Payload.CustomerId + "" : "0";

    }

    public class SmallbankTransaction : ProtobufPayload<SmallbankTransactionPayload>, ITransaction
    {
        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : SmallbankTransactionPayload]\n";

                if (Payload != null)
                {
                    buf += "Payload Type : " + Payload.TxnType + "\n";

                    if(Payload.TxnType == SmallbankTransactionPayload.PayloadType.CreateAccount)
                    {
                        if (Payload.CreateAccount != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id              = {Payload.CreateAccount.CustomerId}\n";
                            buf += $"    Customer Name            = {Payload.CreateAccount.CustomerName}\n";
                            buf += $"    Initial Savings Balance  = {Payload.CreateAccount.InitialSavingsBalance}\n";
                            buf += $"    Initial Checking Balance = {Payload.CreateAccount.InitialCheckingBalance}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == SmallbankTransactionPayload.PayloadType.DepositChecking)
                    {
                        if (Payload.DepositChecking != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.DepositChecking.CustomerId}\n";
                            buf += $"    Amount       = {Payload.DepositChecking.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == SmallbankTransactionPayload.PayloadType.WriteCheck)
                    {
                        if (Payload.WriteCheck != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.WriteCheck.CustomerId}\n";
                            buf += $"    Amount       = {Payload.WriteCheck.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == SmallbankTransactionPayload.PayloadType.TransactSavings)
                    {
                        if (Payload.TransactSavings != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.TransactSavings.CustomerId}\n";
                            buf += $"    Amount       = {Payload.TransactSavings.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == SmallbankTransactionPayload.PayloadType.SendPayment)
                    {
                        if (Payload.SendPayment != null)
                        {
                            buf += "{\n";
                            buf += $"    Source Customer Id       = {Payload.SendPayment.SourceCustomerId}\n";
                            buf += $"    Destination Customer Id  = {Payload.SendPayment.DestCustomerId}\n";
                            buf += $"    Amount                   = {Payload.SendPayment.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == SmallbankTransactionPayload.PayloadType.Amalgamate)
                    {
                        if (Payload.Amalgamate != null)
                        {
                            buf += "{\n";
                            buf += $"    Source Customer Id       = {Payload.Amalgamate.SourceCustomerId}\n";
                            buf += $"    Destination Customer Id  = {Payload.Amalgamate.DestCustomerId}\n";
                            buf += "}\n";
                        }

                    }
                }
                else
                {
                    buf += "<Null Value>";
                }
                return buf;
            }
        }

        public static SmallbankTransaction CreateAccountTransaction(uint cutomerId, string customerName, uint initialiSavingsBalance, uint initialCheckingBalance)
        {
            return new SmallbankTransaction { Payload = SmallbankTransactionPayload.CreateAccountTransactionPayload(cutomerId, customerName, initialiSavingsBalance, initialCheckingBalance) };    
        }

        
        public string? AddressContext
        {
            get
            {
                switch (Payload?.TxnType)
                {
                    case SmallbankTransactionPayload.PayloadType.CreateAccount: return Payload?.CreateAccount?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.DepositChecking: return Payload?.DepositChecking?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.TransactSavings: return Payload?.TransactSavings?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.WriteCheck: return Payload?.WriteCheck?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.SendPayment: return Payload?.SendPayment?.SourceCustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.Amalgamate: return Payload?.Amalgamate?.SourceCustomerId + "";
                }

                return null;
            }
        }

    }

}