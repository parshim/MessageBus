using System.Collections.Generic;
using MessageBus.Core.API;
using Microsoft.AspNet.SignalR.Messaging;

namespace Core.SignalR
{
    public class RabbitScaleoutConfiguration : ScaleoutConfiguration
    {
        public string ConnectionString { get; set; }

        public List<BusHeader> FilterHeaders { get; set; }
    }
}
