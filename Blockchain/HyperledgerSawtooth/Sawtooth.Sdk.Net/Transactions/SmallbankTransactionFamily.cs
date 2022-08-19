using ProtoBuf;
using Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf;
using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class SmallbankTransactionFamily : TransactionFamily
    {

        public SmallbankTransactionFamily(string version) : base("smallbank", version)
        {
            if (version == "1.0")
            {
                SetHandlers(new SmallbankState(), new SmallbankTransaction());
            }
        }

  
    }

    public class SmallbankAddress : IAddress
    {
        public string Prefix => Encoding.UTF8.GetBytes("smallbank").ToSha512().ToHexString().First(6);

        public string ComposeAddress(string context)
        {
            return Prefix + Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().Last(64);
        }

    }

    public class SmallbankState : State
    {
        public SmallbankState() : base(new SmallbankAddress())
        {
        }

        public Account? Account { get; private set; }

        public override string DisplayString {
            get
            {
                string buffer = "[Protobuf Object: Account]\n";
                if (Account != null)
                {
                    buffer += "{\n";
                    buffer += $"    Customer Id      = {Account.CustomerId}\n";
                    buffer += $"    Customer Name    = {Account.CustomerName}\n";
                    buffer += $"    Savings Balance  = {Account.SavingsBalance}\n";
                    buffer += $"    Checking Balance = {Account.CheckingBalance}\n";
                    buffer += "}\n";

                }
                else
                {
                    buffer += "<Null Value>";
                }

                return buffer;
            }
        }

        public override void UnwrapState(string? state_payload)
        {
            if (state_payload == null) return;

            byte[] paylod_raw = Convert.FromBase64String(state_payload);
            using (MemoryStream stream = new MemoryStream(paylod_raw))
            {
                Account = Serializer.Deserialize<Account>(stream);
            }
        }

        public override void WrapState(out string? address, out string? state_payload)
        {
            address = null;
            state_payload = null;
            if (Account != null)
            {
                address = Address.ComposeAddress(Account.CustomerId +"");
                state_payload = Convert.ToBase64String(Account.ToProtobufByteArray());
            }
        }
    }

    public class SmallbankTransaction : ITransaction
    {
        public SmallbankTransactionPayload? SmallbankTransactionPayload { get; set; }

        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : SmallbankTransactionPayload]\n";

                if (SmallbankTransactionPayload != null)
                {
                    buf += "Payload Type : " + SmallbankTransactionPayload.TxnType + "\n";

                    if(SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.CreateAccount)
                    {
                        if (SmallbankTransactionPayload.CreateAccount != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id              = {SmallbankTransactionPayload.CreateAccount.CustomerId}\n";
                            buf += $"    Customer Name            = {SmallbankTransactionPayload.CreateAccount.CustomerName}\n";
                            buf += $"    Initial Savings Balance  = {SmallbankTransactionPayload.CreateAccount.InitialSavingsBalance}\n";
                            buf += $"    Initial Checking Balance = {SmallbankTransactionPayload.CreateAccount.InitialCheckingBalance}\n";
                            buf += "}\n";
                        }
                    }
                    if (SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.DepositChecking)
                    {
                        if (SmallbankTransactionPayload.DepositChecking != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {SmallbankTransactionPayload.DepositChecking.CustomerId}\n";
                            buf += $"    Amount       = {SmallbankTransactionPayload.DepositChecking.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.WriteCheck)
                    {
                        if (SmallbankTransactionPayload.WriteCheck != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {SmallbankTransactionPayload.WriteCheck.CustomerId}\n";
                            buf += $"    Amount       = {SmallbankTransactionPayload.WriteCheck.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.TransactSavings)
                    {
                        if (SmallbankTransactionPayload.TransactSavings != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {SmallbankTransactionPayload.TransactSavings.CustomerId}\n";
                            buf += $"    Amount       = {SmallbankTransactionPayload.TransactSavings.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.SendPayment)
                    {
                        if (SmallbankTransactionPayload.SendPayment != null)
                        {
                            buf += "{\n";
                            buf += $"    Source Customer Id       = {SmallbankTransactionPayload.SendPayment.SourceCustomerId}\n";
                            buf += $"    Destination Customer Id  = {SmallbankTransactionPayload.SendPayment.DestCustomerId}\n";
                            buf += $"    Amount                   = {SmallbankTransactionPayload.SendPayment.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (SmallbankTransactionPayload.TxnType == SmallbankTransactionPayload.PayloadType.Amalgamate)
                    {
                        if (SmallbankTransactionPayload.Amalgamate != null)
                        {
                            buf += "{\n";
                            buf += $"    Source Customer Id       = {SmallbankTransactionPayload.Amalgamate.SourceCustomerId}\n";
                            buf += $"    Destination Customer Id  = {SmallbankTransactionPayload.Amalgamate.DestCustomerId}\n";
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
            return new SmallbankTransaction { SmallbankTransactionPayload = SmallbankTransactionPayload.CreateAccountTransactionPayload(cutomerId, customerName, initialiSavingsBalance, initialCheckingBalance) };    
        }

        public string UnwrapPayload(byte[] payload)
        {
            if (payload == null) return "<Null Payload>";

            using (MemoryStream stream = new MemoryStream(payload))
            {
                SmallbankTransactionPayload = Serializer.Deserialize<SmallbankTransactionPayload>(stream);
            }

            return DisplayString;

        }

        public byte[] WrapPayload()
        {
            if (SmallbankTransactionPayload == null) throw new IOException("Please set 'SmallbankTransactionPayload' before wraping the object.");

            return SmallbankTransactionPayload.ToProtobufByteArray();
        }

        public string? AddressContext
        {
            get
            {
                switch (SmallbankTransactionPayload?.TxnType)
                {
                    case SmallbankTransactionPayload.PayloadType.CreateAccount: return SmallbankTransactionPayload?.CreateAccount?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.DepositChecking: return SmallbankTransactionPayload?.DepositChecking?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.TransactSavings: return SmallbankTransactionPayload?.TransactSavings?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.WriteCheck: return SmallbankTransactionPayload?.WriteCheck?.CustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.SendPayment: return SmallbankTransactionPayload?.SendPayment?.SourceCustomerId + "";
                    case SmallbankTransactionPayload.PayloadType.Amalgamate: return SmallbankTransactionPayload?.Amalgamate?.SourceCustomerId + "";
                }

                return null;
            }
        }

    }

}