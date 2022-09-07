using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions.Families.Smallbank
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
        public string DisplayString
        {
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


        public string AddressContext => Payload?.CustomerId != null ? Payload.CustomerId + "" : "0";

    }

    public class SmallbankTransaction : ProtobufPayload<SmallbankTransactionPayload>, ITransaction
    {

        public SmallbankTransaction() : base()
        {
        }
        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : SmallbankTransactionPayload]\n";

                if (Payload != null)
                {
                    buf += "Payload Type : " + Payload.PayloadType + "\n";

                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.CreateAccount)
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
                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.DepositChecking)
                    {
                        if (Payload.DepositChecking != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.DepositChecking.CustomerId}\n";
                            buf += $"    Amount       = {Payload.DepositChecking.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.WriteCheck)
                    {
                        if (Payload.WriteCheck != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.WriteCheck.CustomerId}\n";
                            buf += $"    Amount       = {Payload.WriteCheck.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.TransactSavings)
                    {
                        if (Payload.TransactSavings != null)
                        {
                            buf += "{\n";
                            buf += $"    Customer Id  = {Payload.TransactSavings.CustomerId}\n";
                            buf += $"    Amount       = {Payload.TransactSavings.Amount}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.SendPayment)
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
                    if (Payload.PayloadType == SmallbankTransactionPayload.Types.PayloadType.Amalgamate)
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

        public static SmallbankTransaction CreateAccountTransaction(uint customerId, string customerName, uint initialiSavingsBalance, uint initialCheckingBalance)
        {
            return new SmallbankTransaction { Payload = CreateAccountTransactionPayload(customerId, customerName, initialiSavingsBalance, initialCheckingBalance) };
        }

        private static SmallbankTransactionPayload CreateAccountTransactionPayload(uint customerId, string customerName, uint initialiSavingsBalance, uint initialCheckingBalance)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.CreateAccount,
                CreateAccount = new SmallbankTransactionPayload.Types.CreateAccountTransactionData
                {
                    CustomerId = customerId,
                    CustomerName = customerName,
                    InitialSavingsBalance = initialiSavingsBalance,
                    InitialCheckingBalance = initialCheckingBalance
                }
            };
        }

        public static SmallbankTransaction CreateDepositCheckTransaction(uint customerId, uint amount)
        {
            return new SmallbankTransaction { Payload = CreateDepositCheckTransactionPayload(customerId, amount) };
        }

        private static SmallbankTransactionPayload CreateDepositCheckTransactionPayload(uint customerId, uint amount)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.DepositChecking,
                DepositChecking = new SmallbankTransactionPayload.Types.DepositCheckingTransactionData
                {
                    CustomerId = customerId,
                    Amount = amount
                }
            };
        }

        public static SmallbankTransaction CreateDepositCashTransaction(uint customerId, int amount)
        {
            return new SmallbankTransaction { Payload = CreateDepositCashTransactionPayload(customerId, amount) };
        }

        private static SmallbankTransactionPayload CreateDepositCashTransactionPayload(uint customerId, int amount)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.TransactSavings,
                TransactSavings = new SmallbankTransactionPayload.Types.TransactSavingsTransactionData
                {
                    CustomerId = customerId,
                    Amount = amount
                }
            };
        }

        public static SmallbankTransaction CreateSendPaymentTransaction(uint customerId, uint destCustomerId, uint amount)
        {
            return new SmallbankTransaction { Payload = CreateSendPaymentTransactionPayload(customerId, destCustomerId, amount) };
        }

        private static SmallbankTransactionPayload CreateSendPaymentTransactionPayload(uint customerId, uint destCustomerId, uint amount)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.SendPayment,
                SendPayment = new SmallbankTransactionPayload.Types.SendPaymentTransactionData
                {
                    SourceCustomerId = customerId,
                    DestCustomerId = destCustomerId,
                    Amount = amount
                }
            };
        }

        public static SmallbankTransaction CreateAmalgamateTransaction(uint customerId, uint destCustomerId)
        {
            return new SmallbankTransaction { Payload = CreateAmalgamateTransactionPayload(customerId, destCustomerId) };
        }

        private static SmallbankTransactionPayload CreateAmalgamateTransactionPayload(uint customerId, uint destCustomerId)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.Amalgamate,
                Amalgamate = new SmallbankTransactionPayload.Types.AmalgamateTransactionData
                {
                    SourceCustomerId = customerId,
                    DestCustomerId = destCustomerId
                }
            };
        }

        public static SmallbankTransaction CreateWriteCheckTransaction(uint customerId, uint amount)
        {
            return new SmallbankTransaction { Payload = CreateWriteCheckTransactionPayload(customerId, amount) };
        }

        private static SmallbankTransactionPayload CreateWriteCheckTransactionPayload(uint customerId, uint amount)
        {
            return new SmallbankTransactionPayload
            {
                PayloadType = SmallbankTransactionPayload.Types.PayloadType.WriteCheck,
                WriteCheck = new SmallbankTransactionPayload.Types.WriteCheckTransactionData
                {
                    CustomerId = customerId,
                    Amount = amount
                }
            };
        }

    }

}