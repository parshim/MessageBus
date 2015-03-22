using System;

using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    internal class ConnectionManager
    {
        private static readonly ConnectionManager PrivateInstance = new ConnectionManager();

        private ConnectionManager()
        {
            
        }

        public static ConnectionManager Instance
        {
            get { return PrivateInstance; }
        }
        
        public void CloseModel(IModel model, TimeSpan timeout)
        {
            model.Close(CurrentVersion.StatusCodes.Ok, "Goodbye");
        }

        public IModel OpenModel(RabbitMQUri uri, IProtocol protocol, TimeSpan timeout)
        {
            IConnection connection = OpenConnection(uri, protocol);

            IModel model = connection.CreateModel();

            connection.AutoClose = true;

            return model;
        }

        private IConnection OpenConnection(RabbitMQUri uri, IProtocol protocol)
        {
            int port = uri.Port.HasValue ? uri.Port.Value : protocol.DefaultPort;

            ConnectionFactory connFactory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = true,
                    HostName = uri.Host,
                    Port = port,
                    Protocol = protocol
                };

            if (uri.Username != null)
            {
                connFactory.UserName = uri.Username;
            }
            if (uri.Password != null)
            {
                connFactory.Password = uri.Password;
            }
            if (uri.VirtualHost != null)
            {
                connFactory.VirtualHost = uri.VirtualHost;
            }

            return connFactory.CreateConnection();
        }
    }
}