using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Messaging
{
    public class ZMQResponse
    {
        public bool IsSuccess { get; set; } 
        public string Error { get; set; } = string.Empty;
        public Message Message { get; set; } = new Message();
    }
}
