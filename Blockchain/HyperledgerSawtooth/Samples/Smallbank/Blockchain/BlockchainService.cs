using Smallbank.Data;

namespace Blockchain
{
    public class BlockchainService : ServiceDescriptor
    {

        public BlockchainService(string url):base(typeof(SmallbankContext), new SmallbankContext(url))
        {
        }
    }
}