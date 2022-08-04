using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.DataHandlers
{
    public interface IDataHandler
    {
        public string UnwrapPayload(string? payload);
    }
}
