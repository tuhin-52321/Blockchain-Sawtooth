using Smallbank.Data;

namespace Blockchain
{
    public class BlockchainService : ServiceDescriptor, IHostedService
    {

        public BlockchainService(string url):base(typeof(SmallbankContext), new SmallbankContext(url))
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Close();
        }

        private  async Task Close()
        {
            SmallbankContext? context = ImplementationInstance as SmallbankContext;

            if(context != null)
            {
                context.Close();
            }
        }
    }
}