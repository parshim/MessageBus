using System;
using System.Collections.Generic;

using RabbitMQ.Client;

namespace MessageBus.Binding.RabbitMQ
{
    internal class ConnectionManager
    {
        private static readonly ConnectionManager PrivateInstance = new ConnectionManager();

        private readonly Dictionary<ConnectionKey, IConnection> _connections = new Dictionary<ConnectionKey, IConnection>();

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
            string host = uri.Host;
            int port = uri.Port.HasValue ? uri.Port.Value : protocol.DefaultPort;

            ConnectionKey key = new ConnectionKey(host, port);

            lock (_connections)
            {
                IConnection connection;
              
                if (_connections.ContainsKey(key))
                {
                    connection = _connections[key];
                }
                else
                {
                    connection = OpenConnection(key, uri, protocol);

                    _connections.Add(key, connection);
                }

                IModel model = connection.CreateModel();

                connection.AutoClose = true;

                return model;
            }
        }

        private IConnection OpenConnection(ConnectionKey key, RabbitMQUri uri, IProtocol protocol)
        {
            ConnectionFactory connFactory = new ConnectionFactory
                {
                    HostName = key.Host,
                    Port = key.Port,
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

            IConnection connection = connFactory.CreateConnection();

            connection.ConnectionShutdown += OnConnectionShutdown;

            return connection;
        }

        private void OnConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            ConnectionKey key = new ConnectionKey(connection.Endpoint.HostName, connection.Endpoint.Port);

            lock (_connections)
            {
                if (_connections.ContainsKey(key))
                {
                    _connections.Remove(key);
                }

                connection.ConnectionShutdown -= OnConnectionShutdown;
            }
        }
    }

    public class ConnectionKey
    {
        private readonly string _host;
        private readonly int _port;

        public ConnectionKey(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public string Host
        {
            get { return _host; }
        }

        public int Port
        {
            get { return _port; }
        }

        protected bool Equals(ConnectionKey other)
        {
            return string.Equals(_host, other._host) && string.Equals(_port, other._port);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConnectionKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_host.GetHashCode()*397) ^ _port.GetHashCode();
            }
        }
    }
}