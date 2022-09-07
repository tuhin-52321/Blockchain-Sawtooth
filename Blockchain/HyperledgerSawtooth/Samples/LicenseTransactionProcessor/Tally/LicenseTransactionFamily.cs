using Google.Protobuf;
using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Utils;


namespace LicenseTransactionProcessor.Tally
{
    public class LicenseTransactionFamily : TransactionFamily<LicenseState, LicenseTransaction>
    {
        private static LicenseHandler handler = new LicenseHandler();
        public LicenseTransactionFamily() : base(handler.FamilyName, handler.Version)
        {

        }

        public override string AddressPrefix => handler.Prefix;

        public override string AddressSuffix(string context)
        {
            return handler.Suffix(context); //LicenseId

        }

    }

    public class LicenseState : ProtobufPayload<License>, IState
    {
        public string DisplayString
        {
            get
            {
                string buffer = "[Protobuf Object: License]\n";
                if (Payload != null)
                {
                    buffer += "{\n";
                    buffer += $"    License Id  = {Payload.LicenseId}\n";
                    buffer += $"    Type        = {Payload.Type}\n";
                    buffer += $"    Created By  = {Payload.CreatedBy}\n";
                    buffer += $"    Approved By = {Payload.ApprovedBy}\n";
                    buffer += $"    Assigned To = {Payload.AssignedTo}\n";
                    buffer += "}\n";

                }
                else
                {
                    buffer += "<Null Value>";
                }

                return buffer;
            }
        }


        public string AddressContext => Payload?.LicenseId != null ? Payload.LicenseId : "";

    }

    public class LicenseTransaction : ProtobufPayload<LicenseTransactionPayload>, ITransaction
    {
        public LicenseTransaction() : base()
        {
        }
        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : LicenseTransaction]\n";

                if (Payload != null)
                {
                    buf += "Payload Type : " + Payload.TxnType + "\n";

                    if (Payload.TxnType == LicenseTxnType.Create)
                    {
                        if (Payload.TxnData != null)
                        {
                            CreateLicense txn_data = Payload.TxnData.ToProtobufClass<CreateLicense>();
                            buf += "{\n";
                            buf += $"    License Id    = {txn_data.LicenseId}\n";
                            buf += $"    Type          = {txn_data.Type}\n";
                            buf += $"    Authorization = {txn_data.Authorization}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == LicenseTxnType.Approve)
                    {
                        if (Payload.TxnData != null)
                        {
                            ApproveLicense txn_data = Payload.TxnData.ToProtobufClass<ApproveLicense>();
                            buf += "{\n";
                            buf += $"    License Id    = {txn_data.LicenseId}\n";
                            buf += $"    Authorization = {txn_data.Authorization}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == LicenseTxnType.Assign)
                    {
                        if (Payload.TxnData != null)
                        {
                            AssignLicense txn_data = Payload.TxnData.ToProtobufClass<AssignLicense>();
                            buf += "{\n";
                            buf += $"    Assignee = {txn_data.Assignee}\n";
                            buf += $"    Type     = {txn_data.Type}\n";
                            buf += "}\n";
                        }
                    }
                    if (Payload.TxnType == LicenseTxnType.Unassign)
                    {
                        if (Payload.TxnData != null)
                        {
                            UnassignLicense txn_data = Payload.TxnData.ToProtobufClass<UnassignLicense>();
                            buf += "{\n";
                            buf += $"    Assignee    = {txn_data.Assignee}\n";
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

        public static LicenseTransaction CreateLicenseTransaction(string licenseId, LicenseType type, string authorization)
        {
            return new LicenseTransaction { Payload = CreateLicenseTransactionPayload(licenseId, type, authorization) };
        }
        public static LicenseTransaction ApproveLicenseTransaction(string licenseId, string authorization)
        {
            return new LicenseTransaction { Payload = ApproveLicenseTransactionPayload(licenseId, authorization) };
        }
        public static LicenseTransaction AssignLicenseTransaction(LicenseType type, string assignee)
        {
            return new LicenseTransaction { Payload = AssignLicenseTransactionPayload(type, assignee) };
        }
        public static LicenseTransaction UnassignLicenseTransaction(string assignee)
        {
            return new LicenseTransaction { Payload = UnassignLicenseTransactionPayload(assignee) };
        }

        private static LicenseTransactionPayload CreateLicenseTransactionPayload(string licenseId, LicenseType type, string authorization)
        {
            return new LicenseTransactionPayload
            {
                TxnType = LicenseTxnType.Create,
                TxnData = new CreateLicense
                {
                    LicenseId = licenseId,
                    Type = type,
                    Authorization = authorization
                }.ToByteString()
            };
        }

        private static LicenseTransactionPayload ApproveLicenseTransactionPayload(string licenseId, string authorization)
        {
            return new LicenseTransactionPayload
            {
                TxnType = LicenseTxnType.Approve,
                TxnData = new ApproveLicense
                {
                    LicenseId = licenseId,
                    Authorization = authorization
                }.ToByteString()
            };
        }
        private static LicenseTransactionPayload AssignLicenseTransactionPayload(LicenseType type, string assignee)
        {
            return new LicenseTransactionPayload
            {
                TxnType = LicenseTxnType.Assign,
                TxnData = new AssignLicense
                {
                    Type = type,
                    Assignee = assignee
                }.ToByteString()
            };
        }
        private static LicenseTransactionPayload UnassignLicenseTransactionPayload(string assignee)
        {
            return new LicenseTransactionPayload
            {
                TxnType = LicenseTxnType.Unassign,
                TxnData = new UnassignLicense
                {
                    Assignee = assignee
                }.ToByteString()
            };
        }


    }

}