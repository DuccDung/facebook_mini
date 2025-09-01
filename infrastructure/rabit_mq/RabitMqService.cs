using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace infrastructure.rabit_mq
{
    public class RabitMqService : IRabitMqService
    {
        private readonly IConnection _connection;
        private readonly RabbitOptions _options;

        public RabitMqService(RabbitOptions options)
        {
            _options = options;
            var factory = new ConnectionFactory()
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Password = options.Password,
                Port = options.Port
            };

            _connection = factory.CreateConnection();
        }

        public void Publish(IModel _channel,TopologyOption topology ,string message, string? routingKey = null, string? exchange = null)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Persistent = true; // để message không mất khi broker restart

            _channel.BasicPublish(
                exchange: exchange ?? topology.Exchange,
                routingKey: routingKey ?? topology.RoutingKey,
                basicProperties: props,
                body: body
            );
        }

        public IModel CreateChannel()
        {
            return _connection.CreateModel();
        }

        public IConnection GetConnection()
        {
            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public void Bind(IModel _channel, TopologyOption topology)
        {
            _channel.QueueBind(topology.Queue, topology.Exchange, topology.RoutingKey);
        }
    }
}
