using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalRtc.Models
{
    public class PayLoad
    {
        public string userToSignal { get; set; }
        public string callerID { get; set; }
        public string signal { get; set; }
    }
}
