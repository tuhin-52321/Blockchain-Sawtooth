﻿using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class XOTransactionFamily : TransactionFamily
    {
        public XOTransactionFamily(string version) : base("xo", version)
        {
            if (version == "1.0")
            {
                SetHandlers(new XOState(), new XOTransaction());
            }
        }

    }

    public class XOAddress : IAddress
    {
        public string Prefix => Encoding.UTF8.GetBytes("xo").ToSha512().ToHexString().First(6);

        public string ComposeAddress(string context)
        {
            return Prefix + Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().First(64);

        }

    }

    public class XOState : State
    {
    
        public XOState() : base(new XOAddress())
        {
        }

        public string? Name { get; private set; }
        public string? Board { get; private set; }
        public string? Status { get; private set; }
        public string? Player1 { get; private set; }
        public string? Player2 { get; private set; }

        public override string DisplayString => "\n"
                 + $"    Game Name   : {Name} \n"
                 + $"    Game Status : {Status} \n"
                 + $"    Player 1    : {Player1} \n"
                 + $"    Player 2    : {Player2} \n"
                 + $"     {Board?[0]} | {Board?[1]} | {Board?[2]}\n"
                 + $"     ---------\n"
                 + $"     {Board?[3]} | {Board?[4]} | {Board?[5]}\n"
                 + $"     ---------\n"
                 + $"     {Board?[6]} | {Board?[7]} | {Board?[8]}\n";


        public override void UnwrapState(string? state_payload)
        {
            if (state_payload == null) return;

            byte[] paylod_raw = Convert.FromBase64String(state_payload);
            string data = Encoding.UTF8.GetString(paylod_raw);

            string[] values = data.Split(",");

            if (values.Length == 5)
            {
                Name = values[0];
                Board = values[1];
                Status = values[2];
                Player1 = values[3];
                Player2 = values[4];
            }
        }

        public override void WrapState(out string? address, out string? state_payload)
        {
            address = null;
            state_payload = null;
            if (Name != null)
            {
                address = Address.ComposeAddress(Name);
                state_payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(Name+","+Board+","+Status+","+Player1+","+Player2));

            }
        }
    }

    public class XOTransaction : ITransaction
    {
        public string? Name { get; set; }
        public string? Action { get; set; }
        public int? Space { get; set; }

        public string DisplayString =>
             "[Comma separated list as string]\n" +
                   $"    Game Name   : {Name} \n"
                 + $"    Action      : {Action} \n"
                 + $"    Space       : {Space} \n";

        public string UnwrapPayload(byte[] payload)
        {
            if (payload == null) return "<Null payload>";


            string data = Encoding.UTF8.GetString(payload);

            string[] values = data.Split(",");

            Name = values[0];
            Action = values[1];

            if (values.Length > 2)
            {
                try
                {
                    Space = int.Parse(values[2]);
                }
                catch
                {
                    Space = null;//not an integer
                }
            }

            return DisplayString;

        }

        public byte[] WrapPayload()
        {
            if (Name == null) throw new IOException("Please set 'Name' before wraping the object.");
            if (Action == null) throw new IOException("Please set 'Action' before wraping the object.");
            if ("take".Equals(Action) && Space == null) throw new IOException("Please set 'Space' for 'take' Action before wraping the object.");

            if ("take".Equals(Action))
                return Encoding.UTF8.GetBytes(Name + "," + Action + "," + Space);
            else
                return Encoding.UTF8.GetBytes(Name + "," + Action + ",");
        }

        public string? AddressContext => Name;
    }
}