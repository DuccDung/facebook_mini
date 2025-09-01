using System.Text;
using RabbitMQ.Client;

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

        public void Publish(IModel _channel,string message, string? routingKey = null, string? exchange = null)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Persistent = true; // để message không mất khi broker restart

            _channel.BasicPublish(
                exchange: exchange ?? _options.Exchange,
                routingKey: routingKey ?? _options.RoutingKey,
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
    }
}
