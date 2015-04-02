using System;

namespace MessageBus.Core
{
    public sealed class RabbitMQUriWellKnownQueryKeys
    {
        public const string RoutingKey = "routingKey";
    }

    /// <summary>
    /// amqp://username:password@localhost:5672/virtualhost/queueORexchange?routingKey=value
    ///  \_/   \_______________/ \_______/ \__/ \_________/ \_____________/ \______________/
    ///   |           |              |       |       |            |                 |                
    ///   |           |      broker hostname |       |            |         Specifies routing key value, may be empty
    ///   |           |                      |       |            |
    ///   |           |                      |  virtual host (optional)
    ///   |           |                      |                    | 
    ///   |           |                      |                    |
    ///   |           |       node port, if absent 5672 is used   |
    ///   |           |                                           |
    ///   |  rabbit mq user info, if absent guest:guest is used   |
    ///   |                                                       |   
    ///   |                                 query name if used for receiving messages
    ///   |                                 exchange name if used for dispatching messages 
    ///scheme  
    /// name                                                    
    /// </summary>
    public sealed class RabbitMQConnectionString
    {
        private readonly UriBuilder _uriBuilder;

        public RabbitMQConnectionString(Uri uri)
        {
            _uriBuilder = new UriBuilder(uri);
        }
        
        public RabbitMQConnectionString()
        {
            _uriBuilder = new UriBuilder("amqp", "localhost");
        }
        
        public RabbitMQConnectionString(string host)
        {
            _uriBuilder = new UriBuilder("amqp", host);
        }

        public RabbitMQConnectionString(string host, int port)
        {
            _uriBuilder = new UriBuilder("amqp", host, port);
        }

        public RabbitMQConnectionString(string host, int port, string endpoint)
        {
            _uriBuilder = new UriBuilder("amqp", host, port, endpoint);
        }

        public string Schema
        {
            get
            {
                return _uriBuilder.Scheme;
            }
            set
            {
                _uriBuilder.Scheme = value;
            }
        }
        
        public string Username
        {
            get
            {
                return _uriBuilder.UserName;
            }
            set
            {
                _uriBuilder.UserName = value;
            }
        }
        
        public string Password
        {
            get
            {
                return _uriBuilder.Password;
            }
            set
            {
                _uriBuilder.Password = value;
            }
        }

        public string Host
        {
            get
            {
                return _uriBuilder.Host;
            }
            set
            {
                _uriBuilder.Host = value;
            }
        }

        public int Port
        {
            get
            {
                return _uriBuilder.Port;
            }
            set
            {
                _uriBuilder.Port = value;
            }
        }

        public string VirtualHost
        {
            get
            {
                string[] segments = _uriBuilder.Path.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries);

                // If there is only one actual segment it will represent exchange or queue name
                if (segments.Length <= 1) return "/";

                // If there is more then one actual segment first will always be virtual host
                return segments[0];
            }
        }

        public string Endpoint
        {
            get
            {
                string[] segments = _uriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // If there is only one actual segment it will represent exchange or queue name
                if (segments.Length == 0) return "";

                // If there is only one actual segment it will represent exchange or queue name
                if (segments.Length <= 1)
                {
                    return segments[0];
                }

                // If there is more then one actual segment first will always be virtual host and second queue or exchange name
                return segments[2];
            }
        }

        public string this[string queryKey]
        {
            get
            {
                string query = _uriBuilder.Query;

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
