using System;

namespace MessageBus.Binding.RabbitMQ
{
    internal sealed class RabbitMQUriWellKnownQueryKeys
    {
        public const string RoutingKey = "routingKey";
    }

    /// <summary>
    /// amqp://username:password@localhost:5672/virtualhost/queueORexchange?routingKey=value
    ///  \_/   \_______________/ \_______/ \__/ \_________/ \_____________/ \______________/
    ///   |           |              |       |       |            |                 |                
    ///   |           |      broker hostname |       |            |         Specifies routing key value, may be empty
    ///   |           |                      |       |            |
    ///   |           |                      |  virtual host, should be absent if rabbit MQ not in cluster mode  
    ///   |           |                      |                    | 
    ///   |           |                      |                    |
    ///   |           |       node port, if absent 5672 is used   |
    ///   |           |                                           |
    ///   |  rabbit mq user info, if absent guest:guest is used   |
    ///   |                                                       |   
    ///   |                                 query name if used for receiving (service) side
    ///   |                                 exchange name if used for dispatching (client) side 
    ///scheme  
    /// name                                                    
    /// </summary>
    internal sealed class RabbitMQUri
    {
        private readonly Uri _uri;

        public RabbitMQUri(Uri uri)
        {
            _uri = uri;
        }

        public string Schema
        {
            get { return _uri.Scheme; }
        }
        
        public string Username
        {
            get
            {
                string userInfo = _uri.UserInfo;

                if (string.IsNullOrEmpty(userInfo)) return null;

                int index = userInfo.IndexOf(':');

                if (index <= 0) return userInfo;

                return userInfo.Substring(0, index + 1);
            }
        }
        
        public string Password
        {
            get
            {
                string userInfo = _uri.UserInfo;

                if (string.IsNullOrEmpty(userInfo)) return null;

                int index = userInfo.IndexOf(':');

                if (index <= 0) return "";

                return userInfo.Substring(index + 1);
            }
        }

        public string Host
        {
            get { return _uri.Host; }
        }

        public int? Port
        {
            get { return _uri.IsDefaultPort ? null : (int?)_uri.Port; }
        }

        public string VirtualHost
        {
            get
            {
                string[] segments = _uri.Segments;

                // First segment is always /
                // If there is only one actual segment it will represent exchange or queue name
                if (segments.Length <= 2) return null;

                // If there is more then one actual segment first will always be virtual host
                return segments[1].Trim('/');
            }
        }

        public string Endpoint
        {
            get
            {
                string[] segments = _uri.Segments;

                // First segment is always /
                // If there is only one actual segment it will represent exchange or queue name
                if (segments.Length <= 1) return null;

                if (segments.Length == 2)
                {
                    return segments[1].Trim('/');
                }

                return segments[2].Trim('/');
            }
        }

        public string this[string queryKey]
        {
            get
            {
                string query = _uri.Query;

                if (string.IsNullOrEmpty(query)) return "";

                foreach (string pair in query.TrimStart('?').Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] strings = pair.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);

                    if (strings[0] == queryKey) return strings[1];
                }

                return "";
            }
        }

        public string RoutingKey
        {
            get { return this[RabbitMQUriWellKnownQueryKeys.RoutingKey]; }
        }

    }
}
