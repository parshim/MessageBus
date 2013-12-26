using System.Configuration;

namespace MessageBus.Core
{
    public class RabbitMQBusConfigSectionHandler : ConfigurationSection
    {
        public const string SectionName = "rabbitMQBus";

        [ConfigurationProperty("brokerHost", DefaultValue = "localhost")]
        public string BrokerHost
        {
            get
            {
                return (string)this["brokerHost"];
            }
            set
            {
                this["brokerHost"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = 5672)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("exchange", DefaultValue = "amq.headers")]
        public string Exchange
        {
            get
            {
                return (string)this["exchange"];
            }
            set
            {
                this["exchange"] = value;
            }
        }
    }
}