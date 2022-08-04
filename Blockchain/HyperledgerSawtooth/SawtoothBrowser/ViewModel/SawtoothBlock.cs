using Sawtooth.Sdk.Net.RESTApi.Payload;
using SawtoothBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothBlock
    {
        public Block? Block { get; private set; }

        public string? BlockNum => Block?.Header?.BlockNum; 
        public string?  BlockId { get; private set; }
        public string? PrevBlockId => Block?.Header?.PreviousBlockId;

        public string BlockIdShort => BlockId.Shorten(16);
        public string PrevBlockIdShort => PrevBlockId.Shorten(16);

        public int? TotalBatches => Block?.Header?.BatchIds.Count;

        public SawtoothBlock(string? blockId, Block? block)
        {
            BlockId = blockId;
            Block = block;
        }

    }
}
