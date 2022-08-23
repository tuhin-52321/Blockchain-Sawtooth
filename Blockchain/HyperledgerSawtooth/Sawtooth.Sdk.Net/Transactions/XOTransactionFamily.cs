
using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class XOTransactionFamily : TransactionFamily<XOState, XOTransaction>
    {
        public XOTransactionFamily() : base("xo", "1.0")
        {
        }

        public override string AddressPrefix => Encoding.UTF8.GetBytes("xo").ToSha512().ToHexString().First(6);

        public override string AddressSuffix(string context)
        {
            return Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().First(64);

        }
    }

 
    public class XOState : CSVStringPayload, IState
    {

        public string AddressContext => Name!=null?Name:"<NotSet>";

        public string? Name { get; private set; }
        public string? Board { get; private set; }
        public string? Status { get; private set; }
        public string? Player1 { get; private set; }
        public string? Player2 { get; private set; }

        public string DisplayString => "\n"
                 + $"    Game Name   : {Name} \n"
                 + $"    Game Status : {Status} \n"
                 + $"    Player 1    : {Player1} \n"
                 + $"    Player 2    : {Player2} \n"
                 + $"     {Board?[0]} | {Board?[1]} | {Board?[2]}\n"
                 + $"     ---------\n"
                 + $"     {Board?[3]} | {Board?[4]} | {Board?[5]}\n"
                 + $"     ---------\n"
                 + $"     {Board?[6]} | {Board?[7]} | {Board?[8]}\n";


        public override void Deserialize(string[] values)
        {
            if (values.Length == 5)
            {
                Name = values[0];
                Board = values[1];
                Status = values[2];
                Player1 = values[3];
                Player2 = values[4];
            }
        }

        public override string Serialize()
        {
            return Name+","+Board+","+Status+","+Player1+","+Player2;

            }
        }
    

    public class XOTransaction : CSVStringPayload, ITransaction
    {
        public string? Name { get; set; }
        public string? Action { get; set; }
        public int? Space { get; set; }

        public string DisplayString =>
             "[Comma separated list as string]\n" +
                   $"    Game Name   : {Name} \n"
                 + $"    Action      : {Action} \n"
                 + $"    Space       : {Space} \n";

        public override void Deserialize(string[] values)
        {

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

        }

        public override string Serialize()
        {
            if (Name == null) throw new IOException("Please set 'Name' before wraping the object.");
            if (Action == null) throw new IOException("Please set 'Action' before wraping the object.");
            if ("take".Equals(Action) && Space == null) throw new IOException("Please set 'Space' for 'take' Action before wraping the object.");

            if ("take".Equals(Action))
                return Name + "," + Action + "," + Space;
            else
                return Name + "," + Action + ",";
        }

        public string? AddressContext => Name;
    }
}