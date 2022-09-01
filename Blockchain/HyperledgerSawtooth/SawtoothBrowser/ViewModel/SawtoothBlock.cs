using SawtoothBrowser.Utils;
using Google.Protobuf;
using System.Linq;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothBlock
    {
        public Block Block { get; private set; }
        public BlockHeader Header { get; private set; } 
        public ulong BlockNum => Header.BlockNum; 
        public string  BlockId { get; private set; }
        public string PrevBlockId => Header.PreviousBlockId;

        public string BlockIdShort => BlockId.Shorten(16);
        public string PrevBlockIdShort => PrevBlockId.Shorten(16);

        public int TotalBatches => Header.BatchIds.Count;

        public SawtoothBlock(string blockId, Block block)
        {
            BlockId = blockId;
            Block = block;
            Header = new BlockHeader();
            Header.MergeFrom(Block.Header);
        }

    }
}
