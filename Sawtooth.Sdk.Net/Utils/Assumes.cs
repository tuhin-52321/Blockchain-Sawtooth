using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Utils
{
    public static class Assumes
    {
        internal static void NotNull<T>(T value, string message)
        {
            if(value == null)
            {
                throw new InternalErrorException(message);
            }
        }
    }
}
