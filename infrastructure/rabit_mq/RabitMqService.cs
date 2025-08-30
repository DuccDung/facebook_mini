using System;
using RabbitMQ.Client;

namespace infrastructure.rabit_mq
{
    internal class RabitMqService : IRabitMqService
    {
        private static IConnection _connection;
        private static IModel _channel;
        private static readonly object LockObject = new object();

        public void ConfigureQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("Connection to RabbitMQ is not established.");
            }

            try
            {
                _channel.QueueDeclare(queue: queueName,
                                      durable: durable,  
                                      exclusive: exclusive,  
                                      autoDelete: autoDelete, 
                                      arguments: null);  

                Console.WriteLine($"Queue '{queueName}' declared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error declaring queue: {ex.Message}");
            }
        }


        public IConnection GetConnection(string hostName, string userName, string password)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                lock (LockObject)
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        var factory = new ConnectionFactory()
                        {
                            HostName = hostName,
                            UserName = userName,
                            Password = password,
                            Port = 5672
                        };
                        _connection = factory.CreateConnection();
                        _channel = _connection.CreateModel();  
                    }
                }
            }
            return _connection;
        }

        public void ReceiveMessage(string queueName)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("Connection to RabbitMQ is not established.");
            }

            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: {message}");
                // Xử lý thông điệp nhận được
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public void SendMessage(string message, string queueName, string exchange = "")
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("Connection to RabbitMQ is not established.");
            }

            var body = System.Text.Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: exchange,
                                  routingKey: queueName,
                                  basicProperties: null,
                                  body: body);

            Console.WriteLine($"Sent: {message}");
        }
    }
}
