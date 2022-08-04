using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.DataHandlers
{
    public static class DataHandlerFactory
    {
        public static IDataHandler GetDataHandler(string? family, string? version)
        {
            if (family != null)
            {
                if (family.Equals("intkey"))
                {
                    return new IntKeyDataHandler(version);
                }
                if (family.Equals("sawtooth_settings"))
                {
                    return new SawtoothSettingsDataHandler(version);
                }
                if (family.Equals("xo"))
                {
                    return new XODataHandler(version);
                }

            }
            return new DefaultDataHandler();
        }
    }
}
