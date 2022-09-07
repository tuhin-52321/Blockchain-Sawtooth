using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Crypto;
using Sawtooth.Sdk.Net.Processor;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Drawing;
using System.Xml.Linq;

namespace LicenseTransactionProcessor.Tally
{
    public class LicenseHandler : ITransactionHandler
    {
        private static Logger log = Logger.GetLogger(typeof(LicenseHandler));
        private T[] Arrayify<T>(T obj) => new[] { obj };

        private const string familyName = "license";

        private readonly string PREFIX = familyName.ToByteArray().ToSha512().ToHexString().Substring(0, 6);

        public string FamilyName => familyName;

        public string Version => "1.0";

        public string Prefix => PREFIX;

        public string Suffix(string licenseId) => licenseId.ToByteArray().ToSha512().TakeLast(32).ToArray().ToHexString();

        public string[] Namespaces => Arrayify(PREFIX);

        private string GetAddress(string licenseId) => Prefix + Suffix(licenseId);

        public async Task ApplyAsync(TpProcessRequest request, TransactionContext context)
        {
            log.Debug("Received process reqest.");
            LicenseTransactionPayload txn = request.Payload.ToByteArray().ToProtobufClass<LicenseTransactionPayload>();

            if (txn.TxnType == LicenseTxnType.Create)
            {
                log.Debug("[Create License]");

                await CreateLicenseAsync(txn.TxnData.ToByteArray().ToProtobufClass<CreateLicense>(), context);
            }

            if (txn.TxnType == LicenseTxnType.Approve)
            {
                log.Debug("[Approve License]");
                await ApproveLicenseAsync(txn.TxnData.ToByteArray().ToProtobufClass<ApproveLicense>(), context);
            }

            if (txn.TxnType == LicenseTxnType.Assign)
            {
                log.Debug("[Assign License]");
                await AssignLicenseAsync(txn.TxnData.ToByteArray().ToProtobufClass<AssignLicense>(), context);
            }

            if (txn.TxnType == LicenseTxnType.Unassign)
            {
                log.Debug("[Unssign License]");
                await UnassignLicenseAsync(txn.TxnData.ToByteArray().ToProtobufClass<UnassignLicense>(), context);
            }
        }

        private async Task CreateLicenseAsync(CreateLicense license, TransactionContext context)
        {
            //TODO : Validate the creator autorization

            var state = await context.GetStateAsync(Arrayify(GetAddress(license.LicenseId)));
            if (state != null && state.Any() && !state.First().Value.IsEmpty)
            {
                log.Error("License '{0}' already exists.", license.LicenseId);
                throw new InvalidTransactionException($"License '{license.LicenseId}' already exists.");
            }
            log.Debug("Adding license state...");
            await context.SetStateAsync(new Dictionary<string, ByteString>
            {
                { GetAddress(license.LicenseId), new License {LicenseId=license.LicenseId, Type = license.Type, CreatedBy = license.Authorization}.ToByteString() }
            }) ;
            log.Info("New license '{0}' created.", license.LicenseId);
        }

        private async Task ApproveLicenseAsync(ApproveLicense license, TransactionContext context)
        {
            //TODO : Validate the approver autorization

            var state = await context.GetStateAsync(Arrayify(GetAddress(license.LicenseId)));
            if (state != null && state.Any() && !state.First().Value.IsEmpty)
            {
                License currentLicense = state.First().Value.ToByteArray().ToProtobufClass<License>();

                //Is it already approved?
                if (!string.IsNullOrEmpty(currentLicense.ApprovedBy))
                {
                    log.Error("License is already approved by '{0}'.", currentLicense.ApprovedBy);
                    throw new InvalidTransactionException($"License is already approved by '{currentLicense.ApprovedBy}'.");
                }
                //Maker/Checker validation
                if (currentLicense.CreatedBy.Equals(license.Authorization))
                {
                    log.Error("License can not be approved by same person who created it.");
                    throw new InvalidTransactionException($"License can not be approved by same person who created it.");
                }

                currentLicense.ApprovedBy = license.Authorization;

                log.Debug("Updating license state...");

                await context.SetStateAsync(new Dictionary<string, ByteString>
                {
                    { state.First().Key, currentLicense.ToByteString() }
                });
                log.Info("License '{0}' is approved.", license.LicenseId);
                return;
            }
            log.Error("License '{0}' not found!", license.LicenseId);
            throw new InvalidTransactionException($"License '{license.LicenseId}' not found!");
        }
        private async Task AssignLicenseAsync(AssignLicense license, TransactionContext context)
        {
            var state = await context.GetStateAsync(Arrayify(PREFIX)); //get all licenses
            License? currentLicense = FindAFreeLicense(state, license.Type, out string key);
            if (currentLicense != null)
            {
                currentLicense.AssignedTo = license.Assignee;

                log.Debug("Updating license state...");
                await context.SetStateAsync(new Dictionary<string, ByteString>
                {
                    { key, currentLicense.ToByteString() }
                });
                log.Info("A '{0}' licence is assigned to '{1}'.", currentLicense.Type, currentLicense.AssignedTo);
                return;
            }
            log.Error("No available '{0}' license  found!", license.Type);
            throw new InvalidTransactionException($"No available '{license.Type}' license  found!");
        }

        private async Task UnassignLicenseAsync(UnassignLicense license, TransactionContext context)
        {
            var state = await context.GetStateAsync(Arrayify(PREFIX)); //get all licenses
            License? currentLicense = FindAssignedLicense(state, license.Assignee, out string key);
            if (currentLicense != null)
            {
                currentLicense.AssignedTo = null;
                log.Debug("Updating license state...");
                await context.SetStateAsync(new Dictionary<string, ByteString>
                {
                    { key, currentLicense.ToByteString() }
                });
                log.Info("A '{0}' licence is unassigned from '{1}'.", currentLicense.Type, license.Assignee);
                return;
            }
            log.Error("No license is assigned to '{0}'.", license.Assignee);

            throw new InvalidTransactionException($"No license is assigned to '{license.Assignee}'.");
        }

        private License? FindAFreeLicense(Dictionary<string, ByteString> state, LicenseType type, out string address)
        {
            if (state != null)
            {
                foreach (string key in state.Keys)
                {
                    ByteString data = state[key];
                    var license = data.ToByteArray().ToProtobufClass<License>();
                    if (license.Type == type && !string.IsNullOrEmpty(license.ApprovedBy) && string.IsNullOrEmpty(license.AssignedTo))
                    {
                        address = key;
                        return license;
                    }
                }
            }
            address = string.Empty;
            return null;
        }

        private License? FindAssignedLicense(Dictionary<string, ByteString> state, string assignee, out string address)
        {
            if (state != null)
            {
                foreach (string key in state.Keys)
                {
                    ByteString data = state[key];
                    var license = data.ToByteArray().ToProtobufClass<License>();
                    if (assignee.Equals(license.AssignedTo))
                    {
                        address = key;
                        return license;
                    }
                }
            }
            address = string.Empty;
            return null;
        }




    }
}