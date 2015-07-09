using System;
using System.Collections.Generic;

namespace MessageBus.Core.API
{
    public class XDeadHeader : BusHeaderBase
    {
        public const string WellknownName = "x-death";

        public XDeadHeader()
        {
            Name = WellknownName;
            RoutingKeys = new List<string>();
        }

        public string Reason { get; set; }
        public string Queue { get; set; }
        public DateTime Time { get; set; }
        public string Exchange { get; set; }
        public IList<string> RoutingKeys { get; private set; }
        
        public override object GetValue()
        {
            // This header is non-sendable

            throw new NotImplementedException();
        }
    }

    public class RejectedHeader : BusHeaderBase
    {
        public const string WellknownName = "bus-reply-rejected";

        public RejectedHeader()
        {
            Name = WellknownName;
        }

        public override object GetValue()
        {
            return "";
        }
    }

    public class ExceptionHeader : BusHeaderBase
    {
        public const string WellknownName = "bus-reply-exception";

        public ExceptionHeader()
        {
            Name = WellknownName;
        }

        public string Message { get; set; }
        
        public override object GetValue()
        {
            return Message;
        }
    }
}