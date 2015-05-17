using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    public class XDeadHeader : BusHeaderBase
    {
        public XDeadHeader()
        {
            Name = "x-dead";
            RoutingKeys = new List<string>();
        }

        public string Reason { get; set; }
        public string Queue { get; set; }
        public DateTime Time { get; set; }
        public string Exchange { get; set; }
        public IList<string> RoutingKeys { get; private set; }
    }
}